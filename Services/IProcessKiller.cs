using System.Collections.Generic;

namespace Pomodoro.Services
{
    /// <summary>
    /// Kills running processes by name. The seam lets <see cref="ProcessBlocker"/> be tested
    /// without spawning or killing real processes.
    /// </summary>
    public interface IProcessKiller
    {
        void Kill(IEnumerable<string> processNames);
    }
}
