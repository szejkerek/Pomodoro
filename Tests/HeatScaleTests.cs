using Pomodoro.Presentation;
using Xunit;

namespace Pomodoro.Tests
{
    public class HeatScaleTests
    {
        private const byte Min = 0x40;

        [Fact]
        public void The_busiest_cell_is_fully_opaque()
        {
            Assert.Equal(0xFF, HeatScale.Alpha(filled: 4, peak: 4, minAlpha: Min));
        }

        [Fact]
        public void An_empty_cell_sits_at_the_minimum_alpha()
        {
            Assert.Equal(Min, HeatScale.Alpha(filled: 0, peak: 4, minAlpha: Min));
        }

        [Fact]
        public void Half_the_peak_lands_between_min_and_full()
        {
            byte alpha = HeatScale.Alpha(filled: 2, peak: 4, minAlpha: Min);

            Assert.True(alpha > Min && alpha < 0xFF);
        }

        [Fact]
        public void A_zero_peak_does_not_divide_by_zero()
        {
            Assert.Equal(Min, HeatScale.Alpha(filled: 0, peak: 0, minAlpha: Min));
        }
    }
}
