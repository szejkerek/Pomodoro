using System.Linq;
using Pomodoro.Models;

namespace Pomodoro.Services
{
    /// <summary>
    /// The cross-module choreography that used to live in the window's event handlers: it drives the
    /// timer, the task list, the completion undo window, and distraction blocking together, so "what
    /// happens when" has one testable home. The window routes actions here and renders from its events.
    /// </summary>
    public sealed class SessionController
    {
        private readonly PomodoroSession session;
        private readonly TaskListModel taskList;
        private readonly PendingCompletions pendingCompletions;
        private readonly FocusGuard focusGuard;
        private readonly SettingsService settings;
        private readonly ITaskGateway gateway;
        private readonly IAutoStart autoStart;

        public SessionController(
            PomodoroSession session,
            TaskListModel taskList,
            PendingCompletions pendingCompletions,
            FocusGuard focusGuard,
            SettingsService settings,
            ITaskGateway gateway,
            IAutoStart autoStart)
        {
            this.session = session;
            this.taskList = taskList;
            this.pendingCompletions = pendingCompletions;
            this.focusGuard = focusGuard;
            this.settings = settings;
            this.gateway = gateway;
            this.autoStart = autoStart;

            session.Changed += UpdateFocusBlock;
            session.Finished += () => Finished?.Invoke(FinishMessage());
            pendingCompletions.Elapsed += taskId => _ = taskList.CloseTaskAsync(taskId);
        }

        /// <summary>A mode just ended; carries the message to show. The session has already advanced.</summary>
        public event Action<string>? Finished;

        public void ToggleStartPause()
        {
            session.ToggleStartPause();
        }

        public void Reset()
        {
            session.Reset();
        }

        public void SwitchMode(TimerMode mode)
        {
            session.SwitchTo(mode);
        }

        /// <summary>
        /// Apply edited settings everywhere they take effect: persist them, update launch-at-login,
        /// reconfigure the task backend, restart the timer on the new durations, and reload the list.
        /// </summary>
        public async Task ApplySettingsAsync()
        {
            settings.Save();
            autoStart.Apply(settings.Current.StartWithWindows);
            gateway.Configure(settings.Current);
            session.ApplySettings();
            await SyncAsync();
        }

        /// <summary>Reload the task list, dropping the active task and any in-flight completion undo first.</summary>
        public async Task SyncAsync()
        {
            pendingCompletions.ClearAll();
            session.ActiveTask = null;
            await taskList.SyncAsync();
        }

        /// <summary>
        /// Click on a task's circle: open the undo window if it's idle, or cancel an open one. The task
        /// actually closes only when the window elapses (see the <see cref="PendingCompletions.Elapsed"/> wiring).
        /// </summary>
        public void ToggleCompletion(string taskId)
        {
            if (pendingCompletions.IsPending(taskId))
            {
                pendingCompletions.Cancel(taskId);
                SetCompleting(taskId, isCompleting: false);
                return;
            }

            TaskItem? task = taskList.Tasks.FirstOrDefault(candidate => candidate.Id == taskId);
            if (task is null)
            {
                return;
            }

            task.IsCompleting = true;
            pendingCompletions.Begin(taskId);
        }

        /// <summary>Mark a task as the one being worked on, clearing any accidental completion on it first.</summary>
        public async Task FocusTaskAsync(string taskId)
        {
            if (pendingCompletions.IsPending(taskId))
            {
                pendingCompletions.Cancel(taskId);
                SetCompleting(taskId, isCompleting: false);
            }

            await taskList.FocusAsync(taskId);

            TaskItem? task = taskList.Tasks.FirstOrDefault(candidate => candidate.Id == taskId);
            session.ActiveTask = task is null ? null : (task.Id, task.Label);
        }

        private void SetCompleting(string taskId, bool isCompleting)
        {
            TaskItem? task = taskList.Tasks.FirstOrDefault(candidate => candidate.Id == taskId);
            if (task is not null)
            {
                task.IsCompleting = isCompleting;
            }
        }

        private void UpdateFocusBlock()
        {
            bool isFocusRunning = session.IsRunning && session.CurrentMode == TimerMode.Pomodoro;
            focusGuard.Update(isFocusRunning);
        }

        private string FinishMessage()
        {
            // The engine has advanced to the next mode, so the upcoming mode tells us what just ended.
            return session.CurrentMode == TimerMode.Pomodoro
                ? "Break's over — back to focus 🍅"
                : "Pomodoro done — take a break ☕";
        }
    }
}
