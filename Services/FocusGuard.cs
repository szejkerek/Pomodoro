namespace Pomodoro.Services
{
    /// <summary>
    /// Turns the per-tick "is a focus session active?" signal into edge-triggered
    /// <see cref="IFocusBlocker"/> calls, so blocking happens once when focus starts and
    /// unblocking once when it ends — never repeatedly on every tick.
    /// </summary>
    public sealed class FocusGuard
    {
        private readonly IFocusBlocker blocker;
        private readonly Func<bool> isEnabled;
        private bool isBlocking;

        public FocusGuard(IFocusBlocker blocker, Func<bool> isEnabled)
        {
            this.blocker = blocker;
            this.isEnabled = isEnabled;
        }

        public void Update(bool isFocusActive)
        {
            bool shouldBlock = isFocusActive && isEnabled();
            if (shouldBlock == isBlocking)
            {
                return;
            }

            isBlocking = shouldBlock;
            if (isBlocking)
            {
                blocker.Block();
                return;
            }

            blocker.Unblock();
        }
    }
}
