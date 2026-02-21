

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
        {
            return;
        }

        _jobHandle = PInvoke.CreateJobObject(null, null);

        if (_jobHandle.IsInvalid)
        {
            return;
        }

        unsafe
        {
            JOBOBJECT_EXTENDED_LIMIT_INFORMATION info = new();
            info.BasicLimitInformation.LimitFlags = JOB_OBJECT_LIMIT.JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE;

            _ = PInvoke.SetInformationJobObject(
                (HANDLE)_jobHandle.DangerousGetHandle(),
                JOBOBJECTINFOCLASS.JobObjectExtendedLimitInformation,
                &info,
                Marshal.SizeOf<JOBOBJECT_EXTENDED_LIMIT_INFORMATION>().To<uint>()
            );
        }
    }

    public async Task<bool> StartProcessAsync(string fileName, string arguments, string workingDirectory, int? ramLimit = null)
    {
        if (IsRunning)
        {
            return false;
        }

        _tcs = new TaskCompletionSource();


        using CancellationTokenRegistration registration = _ct.Register(() => _tcs.TrySetCanceled());

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
                {
                    return;
                }

                bool isSystem = cleanData.Contains("[Server thread/INFO]") || cleanData.Contains("INFO]");


                bool isChat = cleanData.Contains('<') && cleanData.Contains('>');

                if (isSystem && !isChat)
                {

                    if (!_tcs.Task.IsCompleted &&
                        (cleanData.Contains("Done (", StringComparison.OrdinalIgnoreCase) ||
                         cleanData.Contains("Server started.", StringComparison.OrdinalIgnoreCase)))
                    {
                        _ = _tcs.TrySetResult();
                    }

                    if (cleanData.Contains("[Server thread/INFO]") && cleanData.Contains("joined the game", StringComparison.OrdinalIgnoreCase))
                    {
                        _playersInServer++;
                        PlayerCountChanged?.Invoke(_playersInServer);
                    }

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
                {
                    return;
                }

                if (!_tcs.Task.IsCompleted &&
                    (cleanData.Contains("FATAL", StringComparison.OrdinalIgnoreCase) ||
                     cleanData.Contains("ERROR", StringComparison.OrdinalIgnoreCase) ||
                     cleanData.Contains("Exception in thread", StringComparison.OrdinalIgnoreCase)))
                {

                    _ = _tcs.TrySetException(new Exception("El servidor falló al iniciar. Revisa la consola."));
                }
                ErrorReceived?.Invoke(cleanData);
            };

            _process.Exited += (s, e) =>
            {
                if (!_isStopping)
                {
                    int exitCode = _process?.ExitCode ?? 0;
                    Cleanup();
                    if (!_tcs.Task.IsCompleted && _process?.ExitCode != 0)
                    {
                        _ = _tcs.TrySetException(new Exception("El servidor falló al iniciar. Revisa la consola."));
                    }

                    ProcessExited?.Invoke(exitCode == 0, exitCode);
                }
            };

            bool started = _process.Start();

            if (started)
            {
                try
                {
                    if (ramLimit != null && ramLimit > 0 && _jobHandle != null)
                    {
                        ApplyJobLimits(_jobHandle, ramLimit.Value);
                    }
                    EnsureJobObjectCreated();
                    using SafeProcessHandle sProcessHandle = new(_process.Handle, ownsHandle: false);
                    _ = PInvoke.AssignProcessToJobObject(_jobHandle!, sProcessHandle);
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
        {
            return false;
        }

        _isStopping = true;

        try
        {
            SendInput("stop");
            using CancellationTokenSource cts = new(TimeSpan.FromSeconds(10));
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
        _process?.Dispose();
        _process = null;
    }

    private static void ApplyJobLimits(SafeHandle jobHandle, int ramLimit)
    {
        unsafe
        {
            JOBOBJECT_EXTENDED_LIMIT_INFORMATION info = new();

            info.BasicLimitInformation.LimitFlags =
                JOB_OBJECT_LIMIT.JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE |
                JOB_OBJECT_LIMIT.JOB_OBJECT_LIMIT_PROCESS_MEMORY |
                JOB_OBJECT_LIMIT.JOB_OBJECT_LIMIT_WORKINGSET |
                JOB_OBJECT_LIMIT.JOB_OBJECT_LIMIT_PROCESS_TIME |
                JOB_OBJECT_LIMIT.JOB_OBJECT_LIMIT_ACTIVE_PROCESS |
                JOB_OBJECT_LIMIT.JOB_OBJECT_LIMIT_DIE_ON_UNHANDLED_EXCEPTION;

            long bytes = ramLimit.To<long>() * 1024 * 1024;
            info.ProcessMemoryLimit = bytes.To<nuint>();
            info.BasicLimitInformation.MaximumWorkingSetSize = bytes.To<nuint>();
            info.BasicLimitInformation.MinimumWorkingSetSize = bytes.To<nuint>();

            _ = PInvoke.SetInformationJobObject(
                (HANDLE)jobHandle.DangerousGetHandle(),
                JOBOBJECTINFOCLASS.JobObjectExtendedLimitInformation,
                &info,
                sizeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION).To<uint>()
            );
        }
    }

    public void Dispose()
    {
        _jobHandle?.Dispose();

        if (IsRunning)
        {
            _process?.Kill(true);
        }

        Cleanup();
    }

    [GeneratedRegex(@"\x1B\[[0-9;]*[mK]")]
    private static partial Regex AnsiFormattingRegex();
}
