using Pomodoro.Models;
using Pomodoro.Services;
using Xunit;

namespace Pomodoro.Tests
{
    public class TaskListModelTests
    {
        private static SettingsService SettingsWith(AppSettings seed)
        {
            return new SettingsService(new InMemorySettingsStore(seed));
        }

        [Fact]
        public async Task Without_a_token_it_shows_the_token_hint()
        {
            InMemoryTodoistGateway gateway = new InMemoryTodoistGateway();
            TaskListModel model = new TaskListModel(gateway, SettingsWith(new AppSettings()));

            await model.SyncAsync();

            Assert.Equal(TaskListModel.TokenMissingHint, model.Hint);
            Assert.Empty(model.Tasks);
        }

        [Fact]
        public async Task Sync_prepends_all_projects_and_loads_tasks()
        {
            InMemoryTodoistGateway gateway = new InMemoryTodoistGateway();
            gateway.UseToken("t");
            gateway.ProjectsToReturn.Add(new TodoistProject { Id = "P1", Name = "Portfolio" });
            gateway.TasksByKey[""] = new List<TaskItem> { new TaskItem { Id = "1", Label = "a" } };

            TaskListModel model = new TaskListModel(gateway, SettingsWith(new AppSettings()));

            await model.SyncAsync();

            Assert.Equal(2, model.Projects.Count);
            Assert.Equal("All", model.Projects[0].Name);
            Assert.Single(model.Tasks);
        }

        [Fact]
        public async Task A_backend_with_no_projects_reports_none()
        {
            InMemoryTodoistGateway gateway = new InMemoryTodoistGateway();
            gateway.UseToken("t");
            gateway.TasksByKey[""] = new List<TaskItem> { new TaskItem { Id = "1", Label = "a" } };
            TaskListModel model = new TaskListModel(gateway, SettingsWith(new AppSettings()));

            await model.SyncAsync();

            Assert.False(model.HasProjects);
        }

        [Fact]
        public async Task A_backend_with_projects_reports_them()
        {
            InMemoryTodoistGateway gateway = new InMemoryTodoistGateway();
            gateway.UseToken("t");
            gateway.ProjectsToReturn.Add(new TodoistProject { Id = "P1", Name = "Portfolio" });
            TaskListModel model = new TaskListModel(gateway, SettingsWith(new AppSettings()));

            await model.SyncAsync();

            Assert.True(model.HasProjects);
        }

        [Fact]
        public async Task Selected_project_beats_the_filter()
        {
            InMemoryTodoistGateway gateway = new InMemoryTodoistGateway();
            gateway.UseToken("t");
            gateway.ProjectsToReturn.Add(new TodoistProject { Id = "P1", Name = "Portfolio" });
            gateway.TasksByKey["P1"] = new List<TaskItem> { new TaskItem { Id = "byProject", Label = "p" } };
            gateway.TasksByKey["today"] = new List<TaskItem> { new TaskItem { Id = "byFilter", Label = "f" } };

            AppSettings settings = new AppSettings { SelectedProjectId = "P1", TodoistFilter = "today" };
            TaskListModel model = new TaskListModel(gateway, SettingsWith(settings));

            await model.SyncAsync();

            Assert.Single(model.Tasks);
            Assert.Equal("byProject", model.Tasks[0].Id);
        }

        [Fact]
        public async Task Missing_stored_project_falls_back_to_all()
        {
            InMemoryTodoistGateway gateway = new InMemoryTodoistGateway();
            gateway.UseToken("t");
            gateway.ProjectsToReturn.Add(new TodoistProject { Id = "P1", Name = "Portfolio" });
            gateway.TasksByKey[""] = new List<TaskItem>();

            AppSettings settings = new AppSettings { SelectedProjectId = "ghost" };
            TaskListModel model = new TaskListModel(gateway, SettingsWith(settings));

            await model.SyncAsync();

            Assert.Equal(string.Empty, model.SelectedProjectId);
        }

        [Fact]
        public async Task Focusing_a_task_pins_it_to_the_top_and_highlights_only_it()
        {
            InMemoryTodoistGateway gateway = new InMemoryTodoistGateway();
            gateway.UseToken("t");
            gateway.TasksByKey[""] = new List<TaskItem>
            {
                new TaskItem { Id = "1", Label = "a" },
                new TaskItem { Id = "2", Label = "b" },
                new TaskItem { Id = "3", Label = "c" }
            };

            TaskListModel model = new TaskListModel(gateway, SettingsWith(new AppSettings()));
            await model.SyncAsync();

            model.Focus("3");

            Assert.Equal("3", model.Tasks[0].Id);
            Assert.True(model.Tasks[0].IsFocused);
            Assert.All(model.Tasks.Skip(1), task => Assert.False(task.IsFocused));
        }

        [Fact]
        public async Task Focusing_another_task_moves_the_highlight()
        {
            InMemoryTodoistGateway gateway = new InMemoryTodoistGateway();
            gateway.UseToken("t");
            gateway.TasksByKey[""] = new List<TaskItem>
            {
                new TaskItem { Id = "1", Label = "a" },
                new TaskItem { Id = "2", Label = "b" }
            };

            TaskListModel model = new TaskListModel(gateway, SettingsWith(new AppSettings()));
            await model.SyncAsync();

            model.Focus("1");
            model.Focus("2");

            Assert.Equal("2", model.Tasks[0].Id);
            Assert.True(model.Tasks.Single(task => task.Id == "2").IsFocused);
            Assert.False(model.Tasks.Single(task => task.Id == "1").IsFocused);
        }

        [Fact]
        public async Task Focusing_activates_the_task_and_deactivates_the_previous()
        {
            InMemoryTodoistGateway gateway = new InMemoryTodoistGateway();
            gateway.UseToken("t");
            gateway.TasksByKey[""] = new List<TaskItem>
            {
                new TaskItem { Id = "1", Label = "a" },
                new TaskItem { Id = "2", Label = "b" }
            };

            TaskListModel model = new TaskListModel(gateway, SettingsWith(new AppSettings()));
            await model.SyncAsync();

            await model.FocusAsync("1");
            await model.FocusAsync("2");

            Assert.Equal(new[] { "1", "2" }, gateway.ActivatedTaskIds);
            Assert.Equal(new[] { "1" }, gateway.DeactivatedTaskIds);
            Assert.Equal("in progress", model.Tasks.Single(task => task.Id == "2").Status);
            Assert.Equal("to do", model.Tasks.Single(task => task.Id == "1").Status);
        }

        [Fact]
        public async Task Closing_a_task_removes_it_and_calls_the_gateway()
        {
            InMemoryTodoistGateway gateway = new InMemoryTodoistGateway();
            gateway.UseToken("t");
            gateway.ProjectsToReturn.Add(new TodoistProject { Id = "P1", Name = "Portfolio" });
            gateway.TasksByKey[""] = new List<TaskItem>
            {
                new TaskItem { Id = "keep", Label = "k" },
                new TaskItem { Id = "done", Label = "d" }
            };

            TaskListModel model = new TaskListModel(gateway, SettingsWith(new AppSettings()));
            await model.SyncAsync();

            await model.CloseTaskAsync("done");

            Assert.Single(model.Tasks);
            Assert.Equal("keep", model.Tasks[0].Id);
            Assert.Contains("done", gateway.ClosedTaskIds);
        }
    }
}
