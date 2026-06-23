namespace Pomodoro.Services
{
    /// <summary>
    /// The seam under the timer. Production drives it from a DispatcherTimer;
    /// tests drive it by calling <see cref="Advance"/> directly — no real time passes.
    /// </summary>
    public interface IClock
    {
        event Action? Tick;

        /// <summary>Wall-clock time, used to stamp finished sessions. Real in prod, settable in tests.</summary>
        DateTime Now { get; }

        void Start();
        void Stop();
    }
}
