using Pomodoro.Models;
using Pomodoro.Services;
using Xunit;

namespace Pomodoro.Tests
{
    public class SessionLogTests
    {
        [Fact]
        public void A_fresh_log_has_no_entries()
        {
            ISessionLog log = new InMemorySessionLog();

            Assert.Empty(log.All());
        }

        [Fact]
        public void Recording_a_pomodoro_stores_it_with_its_data()
        {
            ISessionLog log = new InMemorySessionLog();
            DateTime when = new DateTime(2026, 6, 23, 14, 30, 0);

            log.Record(new CompletedPomodoro(when, 1500, "task-7"));

            CompletedPomodoro entry = Assert.Single(log.All());
            Assert.Equal(when, entry.CompletedAt);
            Assert.Equal(1500, entry.DurationSeconds);
            Assert.Equal("task-7", entry.TaskId);
        }

        [Fact]
        public void Entries_are_kept_in_the_order_they_were_recorded()
        {
            ISessionLog log = new InMemorySessionLog();
            DateTime first = new DateTime(2026, 6, 23, 9, 0, 0);
            DateTime second = new DateTime(2026, 6, 23, 10, 0, 0);
            DateTime third = new DateTime(2026, 6, 23, 11, 0, 0);

            log.Record(new CompletedPomodoro(first, 1500, null));
            log.Record(new CompletedPomodoro(second, 1500, null));
            log.Record(new CompletedPomodoro(third, 1500, null));

            Assert.Equal(new[] { first, second, third }, log.All().Select(entry => entry.CompletedAt));
        }
    }
}
