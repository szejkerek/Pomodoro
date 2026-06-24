namespace Pomodoro.Services
{
    /// <summary>The three list columns the widget drives, resolved to this list's custom names.</summary>
    public sealed record WorkflowStatuses(string ToDo, string InProgress, string Review);

    /// <summary>
    /// Pure mapping from a ClickUp list's custom statuses onto the three columns the widget drives.
    /// A list's status names are arbitrary, so each column is matched by intent — name fragment first,
    /// then status type, then a sensible literal fallback. No I/O: the gateway fetches the statuses;
    /// this decides what they mean.
    /// </summary>
    public static class ClickUpWorkflow
    {
        private const string ClosedStatusType = "closed";
        private const string DoneStatusType = "done";
        private const string CustomStatusType = "custom";
        private const string UnstartedStatusType = "unstarted";
        private const string ReviewNameFragment = "review";
        private const string InProgressNameFragment = "progress";
        private const string ToDoNameFragment = "to do";
        private const string ToDoNameFragmentAlt = "todo";
        private const string FallbackToDoStatus = "to do";
        private const string FallbackInProgressStatus = "in progress";
        private const string FallbackReviewStatus = "review";

        public static WorkflowStatuses Resolve(IReadOnlyList<(string Name, string Type)> statuses)
        {
            string toDo = NameContaining(statuses, ToDoNameFragment)
                ?? NameContaining(statuses, ToDoNameFragmentAlt)
                ?? OfType(statuses, UnstartedStatusType)
                ?? statuses.Select(status => status.Name).FirstOrDefault()
                ?? FallbackToDoStatus;

            string inProgress = NameContaining(statuses, InProgressNameFragment)
                ?? OfType(statuses, CustomStatusType)
                ?? FallbackInProgressStatus;

            string review = NameContaining(statuses, ReviewNameFragment)
                ?? OfType(statuses, ClosedStatusType)
                ?? OfType(statuses, DoneStatusType)
                ?? FallbackReviewStatus;

            return new WorkflowStatuses(toDo, inProgress, review);
        }

        public static bool IsHidden(string statusName, string statusType, WorkflowStatuses statuses)
        {
            if (statusType == ClosedStatusType || statusType == DoneStatusType)
            {
                return true;
            }

            return string.Equals(statusName, statuses.Review, StringComparison.OrdinalIgnoreCase);
        }

        private static string? NameContaining(IReadOnlyList<(string Name, string Type)> statuses, string fragment)
        {
            return statuses
                .Where(status => status.Name.Contains(fragment, StringComparison.OrdinalIgnoreCase))
                .Select(status => status.Name)
                .FirstOrDefault();
        }

        private static string? OfType(IReadOnlyList<(string Name, string Type)> statuses, string type)
        {
            return statuses
                .Where(status => status.Type == type)
                .Select(status => status.Name)
                .FirstOrDefault();
        }
    }
}
