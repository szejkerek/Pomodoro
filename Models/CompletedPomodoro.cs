namespace Pomodoro.Models
{
    /// <summary>One finished pomodoro. Append-only history row behind <see cref="Pomodoro.Services.ISessionLog"/>.</summary>
    public sealed record CompletedPomodoro(DateTime CompletedAt, int DurationSeconds, string? TaskId);
}
