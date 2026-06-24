using System;
using System.Collections.Generic;
using System.Text;

namespace Pomodoro.Services
{
    /// <summary>
    /// Pure text transforms for the Windows hosts file. Knows how to add and remove a single
    /// clearly-marked "focus block" section without disturbing the user's own entries.
    /// All I/O lives in <see cref="HostsFileBlocker"/>; this class only manipulates strings.
    /// </summary>
    public static class HostsContent
    {
        private const string StartMarker = "# >>> Pomodoro focus block";
        private const string EndMarker = "# <<< Pomodoro focus block";
        private const string BlockedAddress = "0.0.0.0";
        private const string WwwPrefix = "www.";

        /// <summary>Returns <paramref name="existing"/> with a fresh block section appended, replacing any prior one.</summary>
        public static string WithBlock(string existing, IEnumerable<string> domains)
        {
            string stripped = WithoutBlock(existing).TrimEnd('\n');

            StringBuilder section = new StringBuilder();
            section.Append(StartMarker).Append('\n');
            foreach (string domain in domains)
            {
                section.Append(BlockedAddress).Append(' ').Append(domain).Append('\n');
                section.Append(BlockedAddress).Append(' ').Append(WwwPrefix).Append(domain).Append('\n');
            }

            section.Append(EndMarker);

            if (stripped.Length == 0)
            {
                return section.ToString();
            }

            return stripped + "\n" + section.ToString();
        }

        /// <summary>Returns <paramref name="existing"/> with our block section removed; a no-op if none is present.</summary>
        public static string WithoutBlock(string existing)
        {
            int startIndex = existing.IndexOf(StartMarker, StringComparison.Ordinal);
            if (startIndex < 0)
            {
                return existing;
            }

            int endMarkerIndex = existing.IndexOf(EndMarker, startIndex, StringComparison.Ordinal);
            if (endMarkerIndex < 0)
            {
                return existing;
            }

            int removeEnd = endMarkerIndex + EndMarker.Length;
            int removeStart = startIndex;

            // Swallow the single newline we inserted before the section so no blank line is left behind.
            if (removeStart > 0 && existing[removeStart - 1] == '\n')
            {
                removeStart--;
            }

            return existing.Substring(0, removeStart) + existing.Substring(removeEnd);
        }
    }
}
