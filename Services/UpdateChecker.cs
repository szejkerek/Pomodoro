using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pomodoro.Services
{
    /// <summary>
    /// Checks GitHub Releases for a newer build than the running one. Best-effort and quiet: any failure
    /// (offline, rate-limited, malformed tag) returns null so the app never nags or breaks on startup.
    /// </summary>
    public sealed class UpdateChecker
    {
        private const string LatestReleaseUrl = "https://api.github.com/repos/szejkerek/Pomodoro/releases/latest";

        private readonly HttpClient httpClient;

        public UpdateChecker(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public static bool IsNewer(string current, string latest)
        {
            string currentText = current.TrimStart('v', 'V');
            string latestText = latest.TrimStart('v', 'V');

            if (Version.TryParse(currentText, out Version? currentVersion) && Version.TryParse(latestText, out Version? latestVersion))
            {
                return latestVersion > currentVersion;
            }

            return false;
        }

        /// <summary>The latest release tag if it is newer than <paramref name="current"/>, otherwise null.</summary>
        public async Task<string?> LatestNewerThanAsync(string current)
        {
            try
            {
                using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, LatestReleaseUrl);
                request.Headers.UserAgent.ParseAdd("Pomodoro-app");

                using HttpResponseMessage response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode == false)
                {
                    return null;
                }

                string json = await response.Content.ReadAsStringAsync();
                string? tag = JsonSerializer.Deserialize<GitHubRelease>(json)?.TagName;
                if (tag is null || tag.Length == 0)
                {
                    return null;
                }

                return IsNewer(current, tag) ? tag : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private sealed class GitHubRelease
        {
            [JsonPropertyName("tag_name")]
            public string? TagName { get; set; }
        }
    }
}
