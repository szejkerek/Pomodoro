using Pomodoro.Models;

namespace Pomodoro.Services
{
    /// <summary>
    /// The seam over a task backend (Todoist, ClickUp, …). Production talks HTTP;
    /// tests use an in-memory adapter, so the task-list flow runs without a network round-trip.
    /// </summary>
    public interface ITaskGateway
    {
        bool HasToken { get; }

        /// <summary>
        /// Point the gateway at its backend from the live settings. Each adapter reads only what it
        /// needs (Todoist: token + filter; ClickUp: token + list), so callers never reach past the
        /// seam to configure a specific backend.
        /// </summary>
        void Configure(AppSettings settings);

        Task<IReadOnlyList<TodoistProject>> GetProjectsAsync();
        Task<IReadOnlyList<TaskItem>> GetActiveTasksAsync(string filter, string projectId);

        /// <summary>Mark a task as the one being worked on. Returns the status label now shown, or "".</summary>
        Task<string> ActivateTaskAsync(string taskId);

        /// <summary>Return a task to the not-started column. Returns the status label now shown, or "".</summary>
        Task<string> DeactivateTaskAsync(string taskId);

        Task CloseTaskAsync(string taskId);
    }
}
