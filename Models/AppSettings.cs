namespace Pomodoro.Models
{
    public sealed class AppSettings
    {
        public int PomodoroMinutes { get; set; } = 25;
        public int ShortBreakMinutes { get; set; } = 5;
        public int LongBreakMinutes { get; set; } = 15;
        public int LongBreakInterval { get; set; } = 4;

        public bool AutoStartBreaks { get; set; } = false;
        public bool AutoStartPomodoros { get; set; } = false;
        public bool SoundEnabled { get; set; } = true;
        public bool StartWithWindows { get; set; } = true;

        public TaskSource ActiveSource { get; set; } = TaskSource.Todoist;

        public string TodoistToken { get; set; } = string.Empty;
        public string TodoistFilter { get; set; } = string.Empty;
        public string SelectedProjectId { get; set; } = string.Empty;

        public string ClickUpToken { get; set; } = string.Empty;
        public string ClickUpListId { get; set; } = string.Empty;

        public bool BlockDistractionsEnabled { get; set; } = false;
        public string BlockedHosts { get; set; } = DefaultBlockedHosts;
        public string BlockedProcesses { get; set; } = "Spotify";

        public double? WindowLeft { get; set; }
        public double? WindowTop { get; set; }

        private const string DefaultBlockedHosts =
            "youtube.com\nfacebook.com\nlinkedin.com\nmail.google.com\noutlook.live.com\noutlook.office.com";

        /// <summary>The blocked domains, one per line, trimmed and without blanks.</summary>
        public IReadOnlyList<string> BlockedHostList()
        {
            return SplitLines(BlockedHosts, stripExeSuffix: false);
        }

        /// <summary>The blocked process names, one per line, trimmed, with any trailing ".exe" removed.</summary>
        public IReadOnlyList<string> BlockedProcessList()
        {
            return SplitLines(BlockedProcesses, stripExeSuffix: true);
        }

        private static IReadOnlyList<string> SplitLines(string text, bool stripExeSuffix)
        {
            List<string> entries = new List<string>();
            foreach (string rawLine in text.Split('\n'))
            {
                string line = rawLine.Trim();
                if (line.Length == 0)
                {
                    continue;
                }

                if (stripExeSuffix && line.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    line = line.Substring(0, line.Length - ".exe".Length);
                }

                entries.Add(line);
            }

            return entries;
        }

        public int MinutesFor(TimerMode mode)
        {
            if (mode == TimerMode.ShortBreak)
            {
                return ShortBreakMinutes;
            }

            if (mode == TimerMode.LongBreak)
            {
                return LongBreakMinutes;
            }

            return PomodoroMinutes;
        }
    }
}
