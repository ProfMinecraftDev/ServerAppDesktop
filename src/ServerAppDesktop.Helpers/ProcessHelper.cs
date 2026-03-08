namespace ServerAppDesktop.Helpers;

public static class ProcessHelper
{
    private static bool EnsureNonZero(this Windows.Win32.Foundation.BOOL value) => value != 0;

    public static unsafe void SetProcessQualityOfServiceLevel(QualityOfServiceLevel level, Process? process = null)
    {
        PROCESS_POWER_THROTTLING_STATE powerThrottling = new()
        {
            Version = PInvoke.PROCESS_POWER_THROTTLING_CURRENT_VERSION
        };

        switch (level)
        {
            case QualityOfServiceLevel.Default:
                powerThrottling.ControlMask = 0;
                powerThrottling.StateMask = 0;
                break;

            case QualityOfServiceLevel.Eco when Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= 22000:
            case QualityOfServiceLevel.Low:
                powerThrottling.ControlMask = PInvoke.PROCESS_POWER_THROTTLING_EXECUTION_SPEED;
                powerThrottling.StateMask = PInvoke.PROCESS_POWER_THROTTLING_EXECUTION_SPEED;
                break;

            case QualityOfServiceLevel.High:
                powerThrottling.ControlMask = PInvoke.PROCESS_POWER_THROTTLING_EXECUTION_SPEED;
                powerThrottling.StateMask = 0;
                break;

            default:
                throw new NotImplementedException();
        }

        nint handle = process?.Handle ?? PInvoke.GetCurrentProcess();

        _ = PInvoke.SetProcessInformation(
            hProcess: (Windows.Win32.Foundation.HANDLE)handle,
            ProcessInformationClass: PROCESS_INFORMATION_CLASS.ProcessPowerThrottling,
            ProcessInformation: &powerThrottling,
            ProcessInformationSize: (uint)sizeof(PROCESS_POWER_THROTTLING_STATE)).EnsureNonZero();
    }

    public static void SetProcessPriorityClass(ProcessPriorityClass priorityClass, Process? process = null)
    {
        PROCESS_CREATION_FLAGS flags = priorityClass switch
        {
            ProcessPriorityClass.Idle => PROCESS_CREATION_FLAGS.IDLE_PRIORITY_CLASS,
            ProcessPriorityClass.BelowNormal => PROCESS_CREATION_FLAGS.BELOW_NORMAL_PRIORITY_CLASS,
            ProcessPriorityClass.Normal => PROCESS_CREATION_FLAGS.NORMAL_PRIORITY_CLASS,
            ProcessPriorityClass.AboveNormal => PROCESS_CREATION_FLAGS.ABOVE_NORMAL_PRIORITY_CLASS,
            ProcessPriorityClass.High => PROCESS_CREATION_FLAGS.HIGH_PRIORITY_CLASS,
            ProcessPriorityClass.RealTime => PROCESS_CREATION_FLAGS.REALTIME_PRIORITY_CLASS,
            _ => throw new NotImplementedException(),
        };

        nint handle = process?.Handle ?? PInvoke.GetCurrentProcess();

        _ = PInvoke.SetPriorityClass(
            hProcess: (Windows.Win32.Foundation.HANDLE)handle,
            dwPriorityClass: flags).EnsureNonZero();
    }

    public static void SetEfficiencyMode(bool value, Process? process = null)
    {
        QualityOfServiceLevel ecoLevel = Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= 22000
            ? QualityOfServiceLevel.Eco
            : QualityOfServiceLevel.Low;

        SetProcessQualityOfServiceLevel(value
            ? ecoLevel
            : QualityOfServiceLevel.Default, process);

        SetProcessPriorityClass(value
            ? ProcessPriorityClass.Idle
            : ProcessPriorityClass.Normal, process);
    }
}

public enum QualityOfServiceLevel
{
    Default,
    Eco,
    Low,
    High
}
