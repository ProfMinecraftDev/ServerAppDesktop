using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
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

    public event Action<string>? OutputReceived;
    public event Action<string>? ErrorReceived;
    public event Action<bool, int>? ProcessExited;

    public event Action<int>? PlayerJoined;
    public event Action<int>? PlayerLeft;

    public bool IsRunning => _process != null && !_process.HasExited;

    /// <summary>
    /// Crea el Job Object para anclar procesos hijos al proceso padre.
    /// </summary>
    private void EnsureJobObjectCreated()
    {
        if (_jobHandle != null && !_jobHandle.IsInvalid)
            return;

        // 1. Crear el Job
        _jobHandle = PInvoke.CreateJobObject(null, (string?)null);

        if (_jobHandle.IsInvalid)
            return;

        // 2. Configurar el "Auto-Kill"
        unsafe
        {
            JOBOBJECT_EXTENDED_LIMIT_INFORMATION info = new();
            info.BasicLimitInformation.LimitFlags = JOB_OBJECT_LIMIT.JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE;

            // Casteamos el SafeHandle al struct HANDLE que espera la función
            PInvoke.SetInformationJobObject(
                (HANDLE)_jobHandle.DangerousGetHandle(),
                JOBOBJECTINFOCLASS.JobObjectExtendedLimitInformation,
                &info,
                (uint)Marshal.SizeOf<JOBOBJECT_EXTENDED_LIMIT_INFORMATION>()
            );
        }
    }

    public bool StartProcess(string fileName, string arguments, string workingDirectory)
    {
        if (IsRunning)
            return false;

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
                if (e.Data != null)
                {
                    // Solo se ha logrado en Minecraft Bedrock Edition
                    if (e.Data.ToLower().Contains("player connected"))
                    {
                        _playersInServer++;
                        PlayerJoined?.Invoke(_playersInServer);
                    }
                    else if (e.Data.ToLower().Contains("player disconnected"))
                    {
                        _playersInServer--;
                        PlayerLeft?.Invoke(_playersInServer);
                    }
                    OutputReceived?.Invoke(e.Data);
                }
            };
            _process.ErrorDataReceived += (s, e) => { if (e.Data != null) ErrorReceived?.Invoke(e.Data); };

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
                // --- VÍNCULO DE KERNEL (JOB OBJECT) ---
                try
                {
                    EnsureJobObjectCreated();
                    // Usamos SafeProcessHandle para que el PInvoke lo acepte
                    using var sProcessHandle = new SafeProcessHandle(_process.Handle, ownsHandle: false);
                    PInvoke.AssignProcessToJobObject(_jobHandle!, sProcessHandle);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error vinculando al Job: {ex.Message}");
                }
                // --------------------------------------

                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();
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
            using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(10));
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
        // Al cerrar el handle del Job, Windows mata automáticamente al hijo (Java)
        _jobHandle?.Dispose();

        if (IsRunning)
            _process?.Kill(true);

        Cleanup();
    }
}