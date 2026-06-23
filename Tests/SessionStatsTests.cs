using Pomodoro.Models;
using Pomodoro.Services;
using Xunit;

namespace Pomodoro.Tests
{
    public class SessionStatsTests
    {
        private static CompletedPomodoro On(DateTime day)
        {
            return new CompletedPomodoro(day, 1500, null);
        }

        [Fact]
        public void An_empty_log_has_a_streak_of_zero()
        {
            DateTime today = new DateTime(2026, 6, 23);

            Assert.Equal(0, SessionStats.CurrentStreak(Array.Empty<CompletedPomodoro>(), today));
        }

        [Fact]
        public void One_pomodoro_today_is_a_streak_of_one()
        {
            DateTime today = new DateTime(2026, 6, 23);
            CompletedPomodoro[] entries = { On(new DateTime(2026, 6, 23, 14, 0, 0)) };

            Assert.Equal(1, SessionStats.CurrentStreak(entries, today));
        }

        [Fact]
        public void Consecutive_days_ending_today_accumulate()
        {
            DateTime today = new DateTime(2026, 6, 23);
            CompletedPomodoro[] entries =
            {
                On(new DateTime(2026, 6, 21, 8, 0, 0)),
                On(new DateTime(2026, 6, 22, 9, 0, 0)),
                On(new DateTime(2026, 6, 23, 10, 0, 0))
            };

            Assert.Equal(3, SessionStats.CurrentStreak(entries, today));
        }

        [Fact]
        public void A_missed_day_breaks_the_streak()
        {
            DateTime today = new DateTime(2026, 6, 23);
            CompletedPomodoro[] entries =
            {
                On(new DateTime(2026, 6, 21, 8, 0, 0)), // gap on the 22nd
                On(new DateTime(2026, 6, 23, 10, 0, 0))
            };

            Assert.Equal(1, SessionStats.CurrentStreak(entries, today));
        }

        [Fact]
        public void Yesterday_without_today_keeps_the_streak_alive()
        {
            DateTime today = new DateTime(2026, 6, 23);
            CompletedPomodoro[] entries =
            {
                On(new DateTime(2026, 6, 21, 8, 0, 0)),
                On(new DateTime(2026, 6, 22, 9, 0, 0))
            };

            Assert.Equal(2, SessionStats.CurrentStreak(entries, today));
        }

        [Fact]
        public void Two_full_idle_days_reset_the_streak_to_zero()
        {
            DateTime today = new DateTime(2026, 6, 23);
            CompletedPomodoro[] entries = { On(new DateTime(2026, 6, 21, 8, 0, 0)) };

            Assert.Equal(0, SessionStats.CurrentStreak(entries, today));
        }
    }
}
