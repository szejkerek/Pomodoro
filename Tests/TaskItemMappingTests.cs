using Pomodoro.Models;
using Xunit;

namespace Pomodoro.Tests
{
    public class TaskItemMappingTests
    {
        [Fact]
        public void Todoist_dto_maps_id_and_label()
        {
            TodoistTaskDto dto = new TodoistTaskDto { Id = "t1", Content = "Write report" };

            TaskItem item = dto.ToTaskItem();

            Assert.Equal("t1", item.Id);
            Assert.Equal("Write report", item.Label);
        }
    }
}
