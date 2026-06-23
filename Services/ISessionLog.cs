using Pomodoro.Models;

namespace Pomodoro.Services
{
    /// <summary>Append-only history of completed pomodoros. JSON-lines on disk in prod, in-memory in tests.</summary>
    public interface ISessionLog
    {
        void Record(CompletedPomodoro entry);
        IReadOnlyList<CompletedPomodoro> All();
    }
}
