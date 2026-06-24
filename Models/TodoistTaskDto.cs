using System.Text.Json.Serialization;

namespace Pomodoro.Models
{
    /// <summary>The Todoist API task shape. Pure wire: deserialized from JSON, then mapped to a <see cref="TaskItem"/>.</summary>
    public sealed class TodoistTaskDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("priority")]
        public int Priority { get; set; } = 1;

        [JsonPropertyName("child_order")]
        public int ChildOrder { get; set; }

        [JsonPropertyName("is_completed")]
        public bool IsCompleted { get; set; }

        [JsonPropertyName("section_id")]
        public string? SectionId { get; set; }

        [JsonPropertyName("due")]
        public TodoistDue? Due { get; set; }

        public TaskItem ToTaskItem()
        {
            return new TaskItem
            {
                Id = Id,
                Label = Content
            };
        }
    }
}
