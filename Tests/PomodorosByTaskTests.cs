using Pomodoro.Models;
using Pomodoro.Services;
using Xunit;

namespace Pomodoro.Tests
{
    public class PomodorosByTaskTests
    {
        private static CompletedPomodoro For(string? taskId, string? label)
        {
            return new CompletedPomodoro(new DateTime(2026, 6, 23, 10, 0, 0), 1500, taskId, TaskSource.Todoist, label);
        }

        [Fact]
        public void Tasks_are_ranked_by_how_many_pomodoros_they_got()
        {
            CompletedPomodoro[] entries =
            {
                For("a", "Write report"),
                For("b", "Email"),
                For("a", "Write report")
            };

            IReadOnlyList<(string Label, int Count)> ranked = SessionStats.PomodorosByTask(entries);

            Assert.Equal(("Write report", 2), ranked[0]);
            Assert.Equal(("Email", 1), ranked[1]);
        }

        [Fact]
        public void Pomodoros_without_a_task_are_left_out()
        {
            CompletedPomodoro[] entries =
            {
                For(null, null),
                For("a", "Write report")
            };

            IReadOnlyList<(string Label, int Count)> ranked = SessionStats.PomodorosByTask(entries);

            Assert.Equal(("Write report", 1), Assert.Single(ranked));
        }
    }
}
