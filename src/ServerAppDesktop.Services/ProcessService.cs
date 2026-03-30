namespace ServerAppDesktop.Services;

public sealed partial class ProcessService : IProcessService, IDisposable
{
    private Process? _process;
    private bool _isStopping;
    private SafeFileHandle? _jobHandle;
    private int _playersInServer = 0;
    private TaskCompletionSource? _tcs;
    private readonly CancellationToken _ct = CancellationToken.None;

    public event TypedEventHandler<IProcessService, ProcessDataReceivedEventArgs>? DataReceived;
    public event TypedEventHandler<IProcessService, ProcessExitedEventArgs>? ProcessExited;

    public event TypedEventHandler<IProcessService, PlayerCountChangedEventArgs>? PlayerCountChanged;

    public bool IsRunning => _process != null && !_process.HasExited;

    private unsafe void EnsureJobObjectCreated()
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
                sizeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION).To<uint>()
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
                ProcessExited?.Invoke(this, new ProcessExitedEventArgs(false, WIN32_ERROR.ERROR_FILE_NOT_FOUND.To<int>()));
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

                    string? playerName = null;
                    int change = 0;


                    if (cleanData.Contains("joined the game", StringComparison.OrdinalIgnoreCase) ||
                        cleanData.Contains("Player connected", StringComparison.OrdinalIgnoreCase))
                    {
                        change = 1;
                        playerName = cleanData.Split(':').Last()
                            .Replace("joined the game", "", StringComparison.OrdinalIgnoreCase)
                            .Replace("Player connected", "", StringComparison.OrdinalIgnoreCase)
                            .Split(',').First()
                            .Trim();
                    }
                    else if (cleanData.Contains("left the game", StringComparison.OrdinalIgnoreCase) ||
                             cleanData.Contains("Player disconnected", StringComparison.OrdinalIgnoreCase))
                    {
                        change = -1;
                        playerName = cleanData.Split(':').Last()
                            .Replace("left the game", "", StringComparison.OrdinalIgnoreCase)
                            .Replace("Player disconnected", "", StringComparison.OrdinalIgnoreCase)
                            .Split(',').First()
                            .Trim();
                    }

                    if (change != 0 && !string.IsNullOrEmpty(playerName))
                    {
                        _playersInServer = Math.Max(0, _playersInServer + change);
                        PlayerCountChanged?.Invoke(this, new PlayerCountChangedEventArgs(_playersInServer, change, playerName));
                    }
                }

                DataReceived?.Invoke(this, new ProcessDataReceivedEventArgs(cleanData, isError: false));
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

                    _ = _tcs.TrySetException(new Exception(ResourceHelper.GetString("ServerFailedToStart")));
                }
                DataReceived?.Invoke(this, new ProcessDataReceivedEventArgs(cleanData, isError: true));
            };

            _process.Exited += (s, e) =>
            {
                if (!_isStopping)
                {
                    int exitCode = _process?.ExitCode ?? 0;
                    Cleanup();
                    if (!_tcs.Task.IsCompleted && _process?.ExitCode != 0)
                    {
                        _ = _tcs.TrySetException(new Exception(ResourceHelper.GetString("ServerFailedToStart")));
                    }

                    ProcessExited?.Invoke(this, new ProcessExitedEventArgs(exitCode == 0, exitCode));
                }
            };

            bool started = _process.Start();

            if (started)
            {
                try
                {
                    ProcessHelper.SetEfficiencyMode(false, _process);
                    ProcessHelper.SetProcessPriorityClass(ProcessPriorityClass.High, _process);
                    ProcessHelper.SetProcessQualityOfServiceLevel(QualityOfServiceLevel.High, _process);
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
                    Debug.WriteLine(string.Format(ResourceHelper.GetString("ErrorToLinkToJob"), ex.Message));
                }

                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();

                try
                {
                    await _tcs.Task;
                }
                catch
                {
                    return false;
                }
                return true;
            }

            Cleanup();
            return false;
        }
        catch
        {
            Cleanup();
            ProcessExited?.Invoke(this, new ProcessExitedEventArgs(false, -2));
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
            int playersLeaving = -_playersInServer;
            _playersInServer = 0;

            Cleanup();
            _isStopping = false;

            PlayerCountChanged?.Invoke(this, new PlayerCountChangedEventArgs(
                currentPlayers: 0,
                change: playersLeaving
            ));
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
