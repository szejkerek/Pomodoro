using System.Collections.Generic;
using System.Diagnostics;

namespace Pomodoro.Services
{
    /// <summary>Real <see cref="IProcessKiller"/>: kills every running process matching each name.</summary>
    public sealed class ProcessKiller : IProcessKiller
    {
        public void Kill(IEnumerable<string> processNames)
        {
            foreach (string name in processNames)
            {
                KillByName(name);
            }
        }

        private static void KillByName(string name)
        {
            foreach (Process process in Process.GetProcessesByName(name))
            {
                using (process)
                {
                    TryKill(process);
                }
            }
        }

        private static void TryKill(Process process)
        {
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch
            {
                // The process may have already exited, or be one we can't touch — nothing useful to do.
            }
        }
    }
}
