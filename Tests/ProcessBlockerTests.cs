using System.Collections.Generic;
using Pomodoro.Services;
using Xunit;

namespace Pomodoro.Tests
{
    public class ProcessBlockerTests
    {
        /// <summary>Test <see cref="IProcessKiller"/>: records how many times a kill was requested.</summary>
        private sealed class RecordingKiller : IProcessKiller
        {
            public int KillCount { get; private set; }

            public void Kill(IEnumerable<string> processNames)
            {
                KillCount++;
            }
        }

        [Fact]
        public void Block_kills_immediately()
        {
            RecordingKiller killer = new RecordingKiller();
            ProcessBlocker blocker = new ProcessBlocker(new ManualClock(), killer, () => new[] { "Spotify" });

            blocker.Block();

            Assert.Equal(1, killer.KillCount);
        }

        [Fact]
        public void Watchdog_re_kills_on_each_tick()
        {
            ManualClock clock = new ManualClock();
            RecordingKiller killer = new RecordingKiller();
            ProcessBlocker blocker = new ProcessBlocker(clock, killer, () => new[] { "Spotify" });

            blocker.Block();
            clock.Advance(3);

            Assert.Equal(4, killer.KillCount);
        }

        [Fact]
        public void Unblock_stops_the_watchdog()
        {
            ManualClock clock = new ManualClock();
            RecordingKiller killer = new RecordingKiller();
            ProcessBlocker blocker = new ProcessBlocker(clock, killer, () => new[] { "Spotify" });

            blocker.Block();
            blocker.Unblock();
            clock.Advance(3);

            Assert.Equal(1, killer.KillCount);
        }
    }
}
