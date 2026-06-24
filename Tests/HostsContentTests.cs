using Pomodoro.Services;
using Xunit;

namespace Pomodoro.Tests
{
    public class HostsContentTests
    {
        [Fact]
        public void WithBlock_redirects_the_domain_and_its_www_variant()
        {
            string result = HostsContent.WithBlock("# my hosts\n", new[] { "youtube.com" });

            Assert.Contains("0.0.0.0 youtube.com", result);
            Assert.Contains("0.0.0.0 www.youtube.com", result);
        }

        [Fact]
        public void WithBlock_applied_twice_does_not_duplicate_the_section()
        {
            string once = HostsContent.WithBlock("# my hosts\n", new[] { "youtube.com" });
            string twice = HostsContent.WithBlock(once, new[] { "youtube.com" });

            Assert.Equal(once, twice);
            Assert.Equal(1, CountOccurrences(twice, "0.0.0.0 youtube.com\n"));
        }

        [Fact]
        public void WithoutBlock_removes_the_section_but_keeps_user_entries()
        {
            string userHosts = "127.0.0.1 localhost\n1.2.3.4 work.internal\n";
            string blocked = HostsContent.WithBlock(userHosts, new[] { "youtube.com" });

            string restored = HostsContent.WithoutBlock(blocked);

            Assert.DoesNotContain("0.0.0.0 youtube.com", restored);
            Assert.DoesNotContain("Pomodoro focus block", restored);
            Assert.Contains("127.0.0.1 localhost", restored);
            Assert.Contains("1.2.3.4 work.internal", restored);
        }

        [Fact]
        public void WithoutBlock_on_clean_content_is_a_no_op()
        {
            string clean = "127.0.0.1 localhost\n";

            Assert.Equal(clean, HostsContent.WithoutBlock(clean));
        }

        private static int CountOccurrences(string haystack, string needle)
        {
            int count = 0;
            int index = haystack.IndexOf(needle, System.StringComparison.Ordinal);
            while (index >= 0)
            {
                count++;
                index = haystack.IndexOf(needle, index + needle.Length, System.StringComparison.Ordinal);
            }

            return count;
        }
    }
}
