using Pomodoro.Services;
using Xunit;

namespace Pomodoro.Tests
{
    public class FocusGuardTests
    {
        /// <summary>Test <see cref="IFocusBlocker"/>: counts Block/Unblock calls, does nothing real.</summary>
        private sealed class CountingBlocker : IFocusBlocker
        {
            public int BlockCount { get; private set; }
            public int UnblockCount { get; private set; }

            public void Block()
            {
                BlockCount++;
            }

            public void Unblock()
            {
                UnblockCount++;
            }
        }

        [Fact]
        public void Rising_edge_blocks_once()
        {
            CountingBlocker blocker = new CountingBlocker();
            FocusGuard guard = new FocusGuard(blocker, () => true);

            guard.Update(true);

            Assert.Equal(1, blocker.BlockCount);
        }

        [Fact]
        public void Staying_active_does_not_block_again()
        {
            CountingBlocker blocker = new CountingBlocker();
            FocusGuard guard = new FocusGuard(blocker, () => true);

            guard.Update(true);
            guard.Update(true);
            guard.Update(true);

            Assert.Equal(1, blocker.BlockCount);
        }

        [Fact]
        public void Falling_edge_unblocks_once()
        {
            CountingBlocker blocker = new CountingBlocker();
            FocusGuard guard = new FocusGuard(blocker, () => true);

            guard.Update(true);
            guard.Update(false);
            guard.Update(false);

            Assert.Equal(1, blocker.UnblockCount);
        }

        [Fact]
        public void Disabled_never_blocks()
        {
            CountingBlocker blocker = new CountingBlocker();
            FocusGuard guard = new FocusGuard(blocker, () => false);

            guard.Update(true);

            Assert.Equal(0, blocker.BlockCount);
        }
    }
}
