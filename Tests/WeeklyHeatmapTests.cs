using Pomodoro.Models;
using Pomodoro.Services;
using Xunit;

namespace Pomodoro.Tests
{
    public class WeeklyHeatmapTests
    {
        private static CompletedPomodoro At(DateTime moment)
        {
            return new CompletedPomodoro(moment, 1500, null);
        }

        [Fact]
        public void An_empty_log_produces_a_grid_of_zeros()
        {
            int[,] grid = SessionStats.WeeklyHeatmap(Array.Empty<CompletedPomodoro>());

            Assert.Equal(7, grid.GetLength(0));
            Assert.Equal(24, grid.GetLength(1));
            foreach (int count in grid)
            {
                Assert.Equal(0, count);
            }
        }

        [Fact]
        public void A_pomodoro_lands_in_its_day_of_week_and_hour_slot()
        {
            DateTime mondayAfternoon = new DateTime(2026, 6, 22, 14, 5, 0); // Monday

            int[,] grid = SessionStats.WeeklyHeatmap(new[] { At(mondayAfternoon) });

            Assert.Equal(1, grid[(int)DayOfWeek.Monday, 14]);

            int total = 0;
            foreach (int count in grid)
            {
                total += count;
            }
            Assert.Equal(1, total);
        }

        [Fact]
        public void Pomodoros_in_the_same_slot_accumulate_while_other_slots_stay_separate()
        {
            DateTime mondayNine = new DateTime(2026, 6, 22, 9, 0, 0);
            DateTime mondayNineLater = new DateTime(2026, 6, 22, 9, 40, 0);
            DateTime wednesdayTen = new DateTime(2026, 6, 24, 10, 0, 0);

            int[,] grid = SessionStats.WeeklyHeatmap(new[]
            {
                At(mondayNine),
                At(mondayNineLater),
                At(wednesdayTen)
            });

            Assert.Equal(2, grid[(int)DayOfWeek.Monday, 9]);
            Assert.Equal(1, grid[(int)DayOfWeek.Wednesday, 10]);
        }
    }
}
