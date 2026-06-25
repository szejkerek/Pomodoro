using Pomodoro.Models;
using Xunit;

namespace Pomodoro.Tests
{
    public class ConditionalBlockingTests
    {
        private static AppSettings Settings()
        {
            return new AppSettings
            {
                BlockedHosts = "youtube.com",
                BlockedProcesses = "Spotify",
                WorkBlockedHosts = "asana.com\nslack.com",
                WorkBlockedProcesses = "Slack"
            };
        }

        [Fact]
        public void Life_context_blocks_work_tools()
        {
            AppSettings settings = Settings();
            settings.ActiveSource = TaskSource.Todoist;

            Assert.Contains("asana.com", settings.ActiveBlockedHostList());
            Assert.Contains("slack.com", settings.ActiveBlockedHostList());
            Assert.Contains("Slack", settings.ActiveBlockedProcessList());
        }

        [Fact]
        public void Hobby_context_blocks_work_tools()
        {
            AppSettings settings = Settings();
            settings.ActiveSource = TaskSource.ClickUp;

            Assert.Contains("asana.com", settings.ActiveBlockedHostList());
            Assert.Contains("Slack", settings.ActiveBlockedProcessList());
        }

        [Fact]
        public void Work_context_lets_work_tools_through()
        {
            AppSettings settings = Settings();
            settings.ActiveSource = TaskSource.Asana;

            Assert.DoesNotContain("asana.com", settings.ActiveBlockedHostList());
            Assert.DoesNotContain("slack.com", settings.ActiveBlockedHostList());
            Assert.DoesNotContain("Slack", settings.ActiveBlockedProcessList());
        }

        [Fact]
        public void Base_list_is_always_blocked()
        {
            AppSettings settings = Settings();
            settings.ActiveSource = TaskSource.Asana;

            Assert.Contains("youtube.com", settings.ActiveBlockedHostList());
            Assert.Contains("Spotify", settings.ActiveBlockedProcessList());
        }

        [Fact]
        public void Disabling_the_toggle_lets_work_tools_through_in_any_context()
        {
            AppSettings settings = Settings();
            settings.ActiveSource = TaskSource.Todoist;
            settings.BlockWorkToolsOutsideWork = false;

            Assert.DoesNotContain("asana.com", settings.ActiveBlockedHostList());
            Assert.DoesNotContain("Slack", settings.ActiveBlockedProcessList());
        }
    }
}
