using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.JobObjects;

namespace ServerAppDesktop.Services;

public sealed partial class ProcessService : IProcessService, IDisposable
{
    private Process? _process;
    private bool _isStopping;
    private SafeFileHandle? _jobHandle;
    private int _playersInServer = 0;
    private TaskCompletionSource? _tcs;
    private readonly CancellationToken _ct = CancellationToken.None;

    public event Action<string>? OutputReceived;
    public event Action<string>? ErrorReceived;
    public event Action<bool, int>? ProcessExited;

    public event Action<int>? PlayerJoined;
    public event Action<int>? PlayerLeft;

    public bool IsRunning => _process != null && !_process.HasExited;

    private void EnsureJobObjectCreated()
    {
        if (_jobHandle != null && !_jobHandle.IsInvalid)
            return;

        _jobHandle = PInvoke.CreateJobObject(null, (string?)null);

        if (_jobHandle.IsInvalid)
            return;

        unsafe
        {
            JOBOBJECT_EXTENDED_LIMIT_INFORMATION info = new();
            info.BasicLimitInformation.LimitFlags = JOB_OBJECT_LIMIT.JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE;

            PInvoke.SetInformationJobObject(
                (HANDLE)_jobHandle.DangerousGetHandle(),
                JOBOBJECTINFOCLASS.JobObjectExtendedLimitInformation,
                &info,
                (uint)Marshal.SizeOf<JOBOBJECT_EXTENDED_LIMIT_INFORMATION>()
            );
        }
    }

    public async Task<bool> StartProcessAsync(string fileName, string arguments, string workingDirectory)
    {
        if (IsRunning)
            return false;

        _tcs = new TaskCompletionSource();

        // Registramos la cancelación: si el token se cancela, el TCS también falla
        using var registration = _ct.Register(() => _tcs.TrySetCanceled());

        try
        {
            if (Path.IsPathRooted(fileName) && !File.Exists(fileName))
            {
                ProcessExited?.Invoke(false, -1);
                return false;
            }

            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                },
                EnableRaisingEvents = true
            };

            _process.OutputDataReceived += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(e.Data))
                    return;

                // 1. Validamos que sea un mensaje de SISTEMA (ignora lo que digan los jugadores en el chat)
                // Java usa [Server thread/INFO] y Bedrock usa [INFO] al principio
                bool isSystem = e.Data.Contains("[Server thread/INFO]") || e.Data.Contains("INFO]");

                // Si contiene '<' usualmente es un mensaje de chat de jugador en Bedrock o Java Vanilla
                bool isChat = e.Data.Contains('<') && e.Data.Contains('>');

                if (isSystem && !isChat)
                {
                    // 2. ¿Servidor listo?
                    if (!_tcs.Task.IsCompleted &&
                        (e.Data.Contains("Done (", StringComparison.OrdinalIgnoreCase) ||
                         e.Data.Contains("Server started.", StringComparison.OrdinalIgnoreCase)))
                    {
                        _tcs.TrySetResult();
                    }

                    // 3. Contador de jugadores (Solo si la línea NO es un /say o chat)
                    // Buscamos patrones específicos que el servidor imprime al conectar/desconectar
                    if (e.Data.Contains("joined the game", StringComparison.OrdinalIgnoreCase) ||
                        e.Data.Contains("player connected", StringComparison.OrdinalIgnoreCase))
                    {
                        _playersInServer++;
                        PlayerJoined?.Invoke(_playersInServer);
                    }
                    else if (e.Data.Contains("left the game", StringComparison.OrdinalIgnoreCase) ||
                             e.Data.Contains("player disconnected", StringComparison.OrdinalIgnoreCase))
                    {
                        _playersInServer = Math.Max(0, _playersInServer - 1);
                        PlayerLeft?.Invoke(_playersInServer);
                    }
                }

                OutputReceived?.Invoke(e.Data);
            };
            _process.ErrorDataReceived += (s, e) =>
            {
                if (e.Data is not null)
                {
                    if (!_tcs.Task.IsCompleted &&
                        (e.Data.Contains("FATAL", StringComparison.OrdinalIgnoreCase) ||
                         e.Data.Contains("ERROR", StringComparison.OrdinalIgnoreCase) ||
                         e.Data.Contains("Exception in thread", StringComparison.OrdinalIgnoreCase)))
                    {
                        // Si detectamos un error crítico antes de que diga "Done", cancelamos la espera
                        _tcs.TrySetException(new Exception("El servidor falló al iniciar. Revisa la consola."));
                    }
                    ErrorReceived?.Invoke(e.Data);
                }
            };

            _process.Exited += (s, e) =>
            {
                if (!_isStopping)
                {
                    int exitCode = _process?.ExitCode ?? 0;
                    Cleanup();
                    ProcessExited?.Invoke(exitCode == 0, exitCode);
                }
            };

            bool started = _process.Start();

            if (started)
            {
                try
                {
                    EnsureJobObjectCreated();
                    using var sProcessHandle = new SafeProcessHandle(_process.Handle, ownsHandle: false);
                    PInvoke.AssignProcessToJobObject(_jobHandle!, sProcessHandle);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error vinculando al Job: {ex.Message}");
                }

                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();

                await _tcs.Task;
                return true;
            }

            Cleanup();
            return false;
        }
        catch
        {
            Cleanup();
            ProcessExited?.Invoke(false, -2);
            return false;
        }
    }

    public async Task<bool> StopProcessAsync()
    {
        if (!IsRunning || _process == null)
            return false;
        _isStopping = true;

        try
        {
            SendInput("stop");
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await _process.WaitForExitAsync(cts.Token);
            return true;
        }
        catch (OperationCanceledException)
        {
            _process?.Kill(true);
            return true;
        }
        finally
        {
            Cleanup();
            _isStopping = false;
        }
    }

    public void SendInput(string input)
    {
        if (IsRunning && _process != null)
        {
            _process.StandardInput.WriteLine(input);
            _process.StandardInput.Flush();
        }
    }

    private void Cleanup()
    {
        if (_process != null)
        {
            _process.Dispose();
            _process = null;
        }
    }

    public void Dispose()
    {
        _jobHandle?.Dispose();

        if (IsRunning)
            _process?.Kill(true);

        Cleanup();
    }
}