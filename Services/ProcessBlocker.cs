using System.Collections.Generic;

namespace Pomodoro.Services
{
    /// <summary>
    /// Keeps named processes (e.g. Spotify) dead for the duration of a focus session: kills them on
    /// <see cref="Block"/> and re-kills on every clock tick (the watchdog) so reopening them is futile,
    /// until <see cref="Unblock"/> stops the watchdog.
    /// </summary>
    public sealed class ProcessBlocker : IFocusBlocker
    {
        private readonly IClock clock;
        private readonly IProcessKiller killer;
        private readonly Func<IReadOnlyList<string>> processNames;

        public ProcessBlocker(IClock clock, IProcessKiller killer, Func<IReadOnlyList<string>> processNames)
        {
            this.clock = clock;
            this.killer = killer;
            this.processNames = processNames;
            clock.Tick += OnTick;
        }

        public void Block()
        {
            killer.Kill(processNames());
            clock.Start();
        }

        public void Unblock()
        {
            clock.Stop();
        }

        private void OnTick()
        {
            killer.Kill(processNames());
        }
    }
}
