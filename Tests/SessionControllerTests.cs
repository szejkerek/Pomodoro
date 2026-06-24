using Pomodoro.Models;
using Pomodoro.Services;
using Xunit;

namespace Pomodoro.Tests
{
    public class SessionControllerTests
    {
        /// <summary>Test <see cref="IFocusBlocker"/>: counts Block/Unblock calls, does nothing real.</summary>
        private sealed class BlockerSpy : IFocusBlocker
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

        /// <summary>Test <see cref="IAutoStart"/>: records the last applied value.</summary>
        private sealed class RecordingAutoStart : IAutoStart
        {
            public int ApplyCount { get; private set; }
            public bool? LastApplied { get; private set; }

            public bool IsEnabled()
            {
                return LastApplied ?? false;
            }

            public void Apply(bool shouldEnable)
            {
                ApplyCount++;
                LastApplied = shouldEnable;
            }
        }

        /// <summary>Assembles a controller over the in-memory seams, so a test drives the real choreography.</summary>
        private sealed class Harness
        {
            private const int CompletionDelaySeconds = 2;

            public ManualClock SessionClock { get; } = new ManualClock();
            public ManualClock PendingClock { get; } = new ManualClock();
            public InMemoryTodoistGateway Gateway { get; } = new InMemoryTodoistGateway();
            public InMemorySessionLog Log { get; } = new InMemorySessionLog();
            public BlockerSpy Blocker { get; } = new BlockerSpy();
            public RecordingAutoStart AutoStart { get; } = new RecordingAutoStart();
            public SettingsService Settings { get; }
            public TaskListModel TaskList { get; }
            public PomodoroSession Session { get; }
            public PendingCompletions Pending { get; }
            public SessionController Controller { get; }

            public Harness()
            {
                AppSettings seed = new AppSettings
                {
                    PomodoroMinutes = 1,
                    ShortBreakMinutes = 1,
                    LongBreakMinutes = 1,
                    BlockDistractionsEnabled = true,
                    TodoistToken = "seed-token"
                };

                Settings = new SettingsService(new InMemorySettingsStore(seed));
                Gateway.UseToken("seed-token");
                TaskList = new TaskListModel(Gateway, Settings);
                Session = new PomodoroSession(Settings.Current, SessionClock, Log);
                Pending = new PendingCompletions(PendingClock, CompletionDelaySeconds);
                FocusGuard guard = new FocusGuard(Blocker, () => Settings.Current.BlockDistractionsEnabled);
                Controller = new SessionController(Session, TaskList, Pending, guard, Settings, Gateway, AutoStart);
            }
        }

        [Fact]
        public void Running_a_pomodoro_blocks_distractions()
        {
            Harness harness = new Harness();

            harness.Controller.ToggleStartPause();

            Assert.Equal(1, harness.Blocker.BlockCount);
        }

        [Fact]
        public void Pausing_a_pomodoro_unblocks_distractions()
        {
            Harness harness = new Harness();

            harness.Controller.ToggleStartPause();
            harness.Controller.ToggleStartPause();

            Assert.Equal(1, harness.Blocker.UnblockCount);
        }

        [Fact]
        public void Running_a_break_does_not_block_distractions()
        {
            Harness harness = new Harness();

            harness.Controller.SwitchMode(TimerMode.ShortBreak);
            harness.Controller.ToggleStartPause();

            Assert.Equal(0, harness.Blocker.BlockCount);
        }

        [Fact]
        public async Task Sync_clears_the_active_task()
        {
            Harness harness = new Harness();
            harness.Session.ActiveTask = ("t1", "Task one");

            await harness.Controller.SyncAsync();

            Assert.Null(harness.Session.ActiveTask);
        }

        [Fact]
        public async Task Sync_clears_pending_completions()
        {
            Harness harness = new Harness();
            harness.Pending.Begin("t1");

            await harness.Controller.SyncAsync();

            Assert.False(harness.Pending.IsPending("t1"));
        }

        [Fact]
        public async Task Toggling_completion_opens_the_undo_window()
        {
            Harness harness = new Harness();
            harness.Gateway.TasksByKey[""] = new List<TaskItem> { new TaskItem { Id = "t1", Label = "One" } };
            await harness.Controller.SyncAsync();

            harness.Controller.ToggleCompletion("t1");

            Assert.True(harness.Pending.IsPending("t1"));
            Assert.True(harness.TaskList.Tasks.Single().IsCompleting);
        }

        [Fact]
        public async Task Toggling_completion_twice_cancels_without_closing()
        {
            Harness harness = new Harness();
            harness.Gateway.TasksByKey[""] = new List<TaskItem> { new TaskItem { Id = "t1", Label = "One" } };
            await harness.Controller.SyncAsync();

            harness.Controller.ToggleCompletion("t1");
            harness.Controller.ToggleCompletion("t1");

            Assert.False(harness.Pending.IsPending("t1"));
            Assert.False(harness.TaskList.Tasks.Single().IsCompleting);
            Assert.Empty(harness.Gateway.ClosedTaskIds);
        }

        [Fact]
        public async Task Undo_window_elapsing_closes_the_task()
        {
            Harness harness = new Harness();
            harness.Gateway.TasksByKey[""] = new List<TaskItem> { new TaskItem { Id = "t1", Label = "One" } };
            await harness.Controller.SyncAsync();

            harness.Controller.ToggleCompletion("t1");
            harness.PendingClock.Advance(5);

            Assert.Equal(new[] { "t1" }, harness.Gateway.ClosedTaskIds);
        }

        [Fact]
        public async Task Focusing_a_task_stamps_it_as_active()
        {
            Harness harness = new Harness();
            harness.Gateway.TasksByKey[""] = new List<TaskItem> { new TaskItem { Id = "t1", Label = "One" } };
            await harness.Controller.SyncAsync();

            await harness.Controller.FocusTaskAsync("t1");

            Assert.Equal(("t1", "One"), harness.Session.ActiveTask);
        }

        [Fact]
        public async Task Focusing_a_task_cancels_its_pending_completion()
        {
            Harness harness = new Harness();
            harness.Gateway.TasksByKey[""] = new List<TaskItem> { new TaskItem { Id = "t1", Label = "One" } };
            await harness.Controller.SyncAsync();
            harness.Controller.ToggleCompletion("t1");

            await harness.Controller.FocusTaskAsync("t1");

            Assert.False(harness.Pending.IsPending("t1"));
        }

        [Fact]
        public async Task Applying_settings_applies_the_autostart_preference()
        {
            Harness harness = new Harness();
            harness.Settings.Current.StartWithWindows = false;

            await harness.Controller.ApplySettingsAsync();

            Assert.False(harness.AutoStart.LastApplied);
        }

        [Fact]
        public async Task Applying_settings_reloads_the_task_list()
        {
            Harness harness = new Harness();
            harness.Gateway.TasksByKey[""] = new List<TaskItem> { new TaskItem { Id = "t1", Label = "One" } };

            await harness.Controller.ApplySettingsAsync();

            Assert.Single(harness.TaskList.Tasks);
        }

        [Fact]
        public void Finishing_a_pomodoro_announces_a_break()
        {
            Harness harness = new Harness();
            string? message = null;
            harness.Controller.Finished += text => message = text;

            harness.Controller.ToggleStartPause();
            harness.SessionClock.Advance(60);

            Assert.Equal("Pomodoro done — take a break ☕", message);
        }

        [Fact]
        public void Finishing_a_break_announces_focus()
        {
            Harness harness = new Harness();
            string? message = null;
            harness.Controller.Finished += text => message = text;

            harness.Controller.SwitchMode(TimerMode.ShortBreak);
            harness.Controller.ToggleStartPause();
            harness.SessionClock.Advance(60);

            Assert.Equal("Break's over — back to focus 🍅", message);
        }
    }
}
