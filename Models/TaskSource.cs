namespace Pomodoro.Models
{
    /// <summary>
    /// Which backend the task list is currently pulling from — and the context a finished
    /// pomodoro is tagged with: Todoist = life, ClickUp = hobby, Asana = work.
    /// Asana has no task integration; selecting it just declares the work context.
    /// </summary>
    public enum TaskSource
    {
        Todoist = 0,
        ClickUp = 1,
        Asana = 2
    }
}
