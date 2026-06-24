namespace Pomodoro.Services
{
    /// <summary>
    /// Toggles "launch at Windows login". A seam so the settings-applied choreography can be driven
    /// in tests without touching Task Scheduler.
    /// </summary>
    public interface IAutoStart
    {
        bool IsEnabled();
        void Apply(bool shouldEnable);
    }
}
