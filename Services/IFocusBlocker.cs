namespace Pomodoro.Services
{
    /// <summary>
    /// Blocks distractions (sites, apps) while a focus session is running and lifts the block when it ends.
    /// The concrete platform implementations live behind this seam so the timing logic stays testable.
    /// </summary>
    public interface IFocusBlocker
    {
        void Block();
        void Unblock();
    }
}
