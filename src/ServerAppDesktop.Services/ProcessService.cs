using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
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

    public event Action<int>? PlayerCountChanged;

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
                string? cleanData = AnsiFormattingRegex().Replace(e.Data ?? "", string.Empty);

                if (string.IsNullOrWhiteSpace(cleanData))
                    return;

                // 1. Validamos que sea un mensaje de SISTEMA (ignora lo que digan los jugadores en el chat)
                // Java usa [Server thread/INFO] y Bedrock usa [INFO] al principio
                bool isSystem = cleanData.Contains("[Server thread/INFO]") || cleanData.Contains("INFO]");

                // Si contiene '<' usualmente es un mensaje de chat de jugador en Bedrock o Java Vanilla
                bool isChat = cleanData.Contains('<') && cleanData.Contains('>');

                if (isSystem && !isChat)
                {
                    // 2. ¿Servidor listo?
                    if (!_tcs.Task.IsCompleted &&
                        (cleanData.Contains("Done (", StringComparison.OrdinalIgnoreCase) ||
                         cleanData.Contains("Server started.", StringComparison.OrdinalIgnoreCase)))
                    {
                        _tcs.TrySetResult();
                    }

                    // 3. Contador de jugadores (Lógica unificada para Arclight + Geyser)
                    // Filtramos para que solo cuente cuando el hilo oficial de Java (Server thread) de la orden.

                    // Detectamos la entrada
                    if (cleanData.Contains("[Server thread/INFO]") && cleanData.Contains("joined the game", StringComparison.OrdinalIgnoreCase))
                    {
                        // Esto atrapará tanto a "Player" como a ".ProMinecraft426" una sola vez.
                        _playersInServer++;
                        PlayerCountChanged?.Invoke(_playersInServer);
                    }
                    // Detectamos la salida
                    else if (cleanData.Contains("[Server thread/INFO]") && cleanData.Contains("left the game", StringComparison.OrdinalIgnoreCase))
                    {
                        _playersInServer = Math.Max(0, _playersInServer - 1);
                        PlayerCountChanged?.Invoke(_playersInServer);
                    }
                }

                OutputReceived?.Invoke(cleanData);
            };
            _process.ErrorDataReceived += (s, e) =>
            {
                string? cleanData = AnsiFormattingRegex().Replace(e.Data ?? "", string.Empty);

                if (string.IsNullOrWhiteSpace(cleanData))
                    return;

                if (!_tcs.Task.IsCompleted &&
                    (cleanData.Contains("FATAL", StringComparison.OrdinalIgnoreCase) ||
                     cleanData.Contains("ERROR", StringComparison.OrdinalIgnoreCase) ||
                     cleanData.Contains("Exception in thread", StringComparison.OrdinalIgnoreCase)))
                {
                    // Si detectamos un error crítico antes de que diga "Done", cancelamos la espera
                    _tcs.TrySetException(new Exception("El servidor falló al iniciar. Revisa la consola."));
                }
                ErrorReceived?.Invoke(cleanData);
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
            PlayerCountChanged?.Invoke(0);
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

    [GeneratedRegex(@"\x1B\[[0-9;]*[mK]")]
    private static partial Regex AnsiFormattingRegex();
}