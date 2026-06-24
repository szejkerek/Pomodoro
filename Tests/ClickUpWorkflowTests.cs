using Pomodoro.Services;
using Xunit;

namespace Pomodoro.Tests
{
    public class ClickUpWorkflowTests
    {
        [Fact]
        public void Resolves_columns_by_name_fragment()
        {
            List<(string Name, string Type)> statuses = new List<(string, string)>
            {
                ("To Do", "open"),
                ("In Progress", "custom"),
                ("In Review", "closed")
            };

            WorkflowStatuses workflow = ClickUpWorkflow.Resolve(statuses);

            Assert.Equal("To Do", workflow.ToDo);
            Assert.Equal("In Progress", workflow.InProgress);
            Assert.Equal("In Review", workflow.Review);
        }

        [Fact]
        public void Falls_back_to_status_type_when_names_do_not_match()
        {
            List<(string Name, string Type)> statuses = new List<(string, string)>
            {
                ("Backlog", "unstarted"),
                ("Doing", "custom"),
                ("Complete", "done")
            };

            WorkflowStatuses workflow = ClickUpWorkflow.Resolve(statuses);

            Assert.Equal("Backlog", workflow.ToDo);
            Assert.Equal("Doing", workflow.InProgress);
            Assert.Equal("Complete", workflow.Review);
        }

        [Fact]
        public void Falls_back_to_literals_when_the_list_is_empty()
        {
            WorkflowStatuses workflow = ClickUpWorkflow.Resolve(new List<(string Name, string Type)>());

            Assert.Equal("to do", workflow.ToDo);
            Assert.Equal("in progress", workflow.InProgress);
            Assert.Equal("review", workflow.Review);
        }

        [Fact]
        public void Closed_and_done_and_review_statuses_are_hidden()
        {
            WorkflowStatuses workflow = new WorkflowStatuses("To Do", "In Progress", "In Review");

            Assert.True(ClickUpWorkflow.IsHidden("anything", "closed", workflow));
            Assert.True(ClickUpWorkflow.IsHidden("anything", "done", workflow));
            Assert.True(ClickUpWorkflow.IsHidden("In Review", "custom", workflow));
        }

        [Fact]
        public void Open_and_in_progress_statuses_are_visible()
        {
            WorkflowStatuses workflow = new WorkflowStatuses("To Do", "In Progress", "In Review");

            Assert.False(ClickUpWorkflow.IsHidden("To Do", "open", workflow));
            Assert.False(ClickUpWorkflow.IsHidden("In Progress", "custom", workflow));
        }
    }
}
