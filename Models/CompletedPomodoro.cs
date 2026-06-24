namespace Pomodoro.Models
{
    /// <summary>
    /// One finished pomodoro. Append-only history row behind <see cref="Pomodoro.Services.ISessionLog"/>.
    /// <paramref name="Source"/> is the context it was completed under (life/hobby/work); it defaults to
    /// Todoist so history written before sources existed still deserializes.
    /// </summary>
    public sealed record CompletedPomodoro(
        DateTime CompletedAt,
        int DurationSeconds,
        string? TaskId,
        TaskSource Source = TaskSource.Todoist);
}
