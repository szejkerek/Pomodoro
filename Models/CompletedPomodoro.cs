namespace Pomodoro.Models
{
    /// <summary>
    /// One finished pomodoro. Append-only history row behind <see cref="Pomodoro.Services.ISessionLog"/>.
    /// <paramref name="Source"/> is the context it was completed under (life/hobby/work); it defaults to
    /// Todoist so history written before sources existed still deserializes. <paramref name="TaskLabel"/>
    /// is the focused task's title at completion (null when none), kept so stats can name a task whose
    /// row is long gone.
    /// </summary>
    public sealed record CompletedPomodoro(
        DateTime CompletedAt,
        int DurationSeconds,
        string? TaskId,
        TaskSource Source = TaskSource.Todoist,
        string? TaskLabel = null);
}
