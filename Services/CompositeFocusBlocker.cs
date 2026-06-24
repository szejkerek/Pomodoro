using System.Collections.Generic;

namespace Pomodoro.Services
{
    /// <summary>
    /// Fans <see cref="Block"/>/<see cref="Unblock"/> out to several blockers (sites + apps). Each call is
    /// isolated so a failure in one (e.g. the hosts file) can't leave the others stranded.
    /// </summary>
    public sealed class CompositeFocusBlocker : IFocusBlocker
    {
        private readonly IReadOnlyList<IFocusBlocker> blockers;

        public CompositeFocusBlocker(params IFocusBlocker[] blockers)
        {
            this.blockers = blockers;
        }

        public void Block()
        {
            foreach (IFocusBlocker blocker in blockers)
            {
                TryRun(blocker.Block);
            }
        }

        public void Unblock()
        {
            foreach (IFocusBlocker blocker in blockers)
            {
                TryRun(blocker.Unblock);
            }
        }

        private static void TryRun(System.Action action)
        {
            try
            {
                action();
            }
            catch
            {
                // One blocker failing must not stop the others; nothing actionable to report here.
            }
        }
    }
}
