using System.Net;
using System.Net.Http;
using Pomodoro.Services;
using Xunit;

namespace Pomodoro.Tests
{
    public class UpdateCheckerTests
    {
        [Theory]
        [InlineData("1.0.0", "1.0.1", true)]
        [InlineData("1.0.0", "v1.1.0", true)]   // tolerates a "v" prefix
        [InlineData("1.2.0", "1.10.0", true)]   // numeric, not lexicographic
        [InlineData("1.0.0", "1.0.0", false)]
        [InlineData("2.0.0", "1.9.9", false)]
        [InlineData("1.0.0", "garbage", false)] // never nag on a bad tag
        public void Newer_versions_are_recognised(string current, string latest, bool expected)
        {
            Assert.Equal(expected, UpdateChecker.IsNewer(current, latest));
        }

        [Fact]
        public async Task Returns_the_tag_when_the_release_is_newer()
        {
            QueuedHandler handler = new QueuedHandler();
            handler.Enqueue(HttpStatusCode.OK, "{\"tag_name\":\"v1.1.0\"}");
            UpdateChecker checker = new UpdateChecker(new HttpClient(handler));

            Assert.Equal("v1.1.0", await checker.LatestNewerThanAsync("1.0.0"));
        }

        [Fact]
        public async Task Returns_null_when_already_up_to_date()
        {
            QueuedHandler handler = new QueuedHandler();
            handler.Enqueue(HttpStatusCode.OK, "{\"tag_name\":\"v1.0.0\"}");
            UpdateChecker checker = new UpdateChecker(new HttpClient(handler));

            Assert.Null(await checker.LatestNewerThanAsync("1.0.0"));
        }
    }
}
