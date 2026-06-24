using System.Diagnostics;
using Microsoft.Win32;

namespace Pomodoro.Services
{
    /// <summary>
    /// Toggles "launch at Windows login". Because the app runs elevated (for the hosts-file block),
    /// the per-user <c>Run</c> key can't start it — Windows refuses to auto-launch elevated apps that way.
    /// So we register a Task Scheduler logon task at the highest run level instead, via <c>schtasks.exe</c>
    /// (no extra dependencies), and clean up any leftover legacy <c>Run</c> value.
    /// </summary>
    public sealed class AutoStartManager
    {
        private const string TaskName = "Pomodoro";
        private const string LegacyRunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string LegacyValueName = "Pomodoro";

        public bool IsEnabled()
        {
            return RunSchTasks($"/Query /TN {TaskName}") == 0;
        }

        public void Apply(bool shouldEnable)
        {
            RemoveLegacyRunEntry();

            if (shouldEnable)
            {
                string executablePath = GetExecutablePath();
                RunSchTasks($"/Create /TN {TaskName} /SC ONLOGON /RL HIGHEST /TR \"\\\"{executablePath}\\\"\" /F");
                return;
            }

            RunSchTasks($"/Delete /TN {TaskName} /F");
        }

        private static int RunSchTasks(string arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo("schtasks.exe", arguments)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using Process? process = Process.Start(startInfo);
            if (process is null)
            {
                return -1;
            }

            process.WaitForExit();
            return process.ExitCode;
        }

        private static void RemoveLegacyRunEntry()
        {
            using RegistryKey? runKey = Registry.CurrentUser.OpenSubKey(LegacyRunKeyPath, writable: true);
            if (runKey is null)
            {
                return;
            }

            if (runKey.GetValue(LegacyValueName) is not null)
            {
                runKey.DeleteValue(LegacyValueName);
            }
        }

        private static string GetExecutablePath()
        {
            return Process.GetCurrentProcess().MainModule?.FileName ?? Environment.ProcessPath ?? string.Empty;
        }
    }
}
