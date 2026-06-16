using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SubProcessDemo.Host.Pool;

/// <summary>
/// Windows Job Object 绑定：主进程退出时，所有子进程自动终止。
/// 对应设计文档中的 ChildProcessHelper。
/// </summary>
public static class JobObjectHelper
{
    private static readonly IntPtr _jobHandle;

    static JobObjectHelper()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

        var jobName = $"SubProcessDemo_Job_{Environment.ProcessId}";
        _jobHandle = CreateJobObject(IntPtr.Zero, jobName);

        if (_jobHandle == IntPtr.Zero) return;

        // 设置 JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE
        var info = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION
        {
            BasicLimitInformation = new JOBOBJECT_BASIC_LIMIT_INFORMATION
            {
                LimitFlags = 0x00002000 // JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE
            }
        };

        SetInformationJobObject(
            _jobHandle,
            JobObjectInfoClass.JobObjectExtendedLimitInformation,
            ref info,
            Marshal.SizeOf<JOBOBJECT_EXTENDED_LIMIT_INFORMATION>());
    }

    /// <summary>将子进程绑定到 Job Object</summary>
    public static void AddProcess(Process process)
    {
        if (_jobHandle == IntPtr.Zero) return;
        AssignProcessToJobObject(_jobHandle, process.Handle);
    }

    // ── P/Invoke ──

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr CreateJobObject(IntPtr lpJobAttributes, string? lpName);

    [DllImport("kernel32.dll")]
    private static extern bool SetInformationJobObject(
        IntPtr hJob,
        JobObjectInfoClass infoClass,
        ref JOBOBJECT_EXTENDED_LIMIT_INFORMATION lpInfo,
        int cbInfoLength);

    [DllImport("kernel32.dll")]
    private static extern bool AssignProcessToJobObject(IntPtr hJob, IntPtr hProcess);

    private enum JobObjectInfoClass
    {
        JobObjectExtendedLimitInformation = 9
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct JOBOBJECT_BASIC_LIMIT_INFORMATION
    {
        public long PerProcessUserTimeLimit;
        public long PerJobUserTimeLimit;
        public uint LimitFlags;
        public IntPtr MinimumWorkingSetSize;
        public IntPtr MaximumWorkingSetSize;
        public uint ActiveProcessLimit;
        public IntPtr Affinity;
        public uint PriorityClass;
        public uint SchedulingClass;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct IO_COUNTERS
    {
        public ulong ReadOperationCount;
        public ulong WriteOperationCount;
        public ulong OtherOperationCount;
        public ulong ReadTransferCount;
        public ulong WriteTransferCount;
        public ulong OtherTransferCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
    {
        public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
        public IO_COUNTERS IoInfo;
        public IntPtr ProcessMemoryLimit;
        public IntPtr JobMemoryLimit;
        public IntPtr PeakProcessMemoryUsed;
        public IntPtr PeakJobMemoryUsed;
    }
}
