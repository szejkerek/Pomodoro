using System.ComponentModel;

namespace Pomodoro.Models
{
    /// <summary>
    /// A task as the UI sees it — vendor-neutral and bindable. Backends produce these from their own
    /// wire shapes (e.g. <see cref="TodoistTaskDto"/>), so the window never binds to a backend's DTO.
    /// </summary>
    public sealed class TaskItem : INotifyPropertyChanged
    {
        public string Id { get; init; } = string.Empty;
        public string Label { get; init; } = string.Empty;

        private bool isFocused;
        private bool isCompleting;
        private string status = string.Empty;
        private string sectionName = string.Empty;
        private string dueDate = string.Empty;

        /// <summary>The backend status/column this task sits in (ClickUp), e.g. "in progress". Empty otherwise.</summary>
        public string Status
        {
            get => status;
            set => SetField(ref status, value, nameof(Status));
        }

        /// <summary>The section this task sits in (Todoist), shown for information. Empty if none.</summary>
        public string SectionName
        {
            get => sectionName;
            set => SetField(ref sectionName, value, nameof(SectionName));
        }

        /// <summary>Short due-date label (e.g. "📅 Jun 26"), or empty when the task has no due date.</summary>
        public string DueDate
        {
            get => dueDate;
            set => SetField(ref dueDate, value, nameof(DueDate));
        }

        /// <summary>The task the user is currently working on (pinned to the top, highlighted).</summary>
        public bool IsFocused
        {
            get => isFocused;
            set => SetField(ref isFocused, value, nameof(IsFocused));
        }

        /// <summary>A completion is pending (the undo window is open and the row is fading out).</summary>
        public bool IsCompleting
        {
            get => isCompleting;
            set => SetField(ref isCompleting, value, nameof(IsCompleting));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
