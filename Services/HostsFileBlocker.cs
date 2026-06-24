using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Pomodoro.Services
{
    /// <summary>
    /// Real <see cref="IFocusBlocker"/> for websites: writes a marked block of <c>0.0.0.0</c> redirects
    /// into the Windows hosts file and removes it again, flushing the DNS cache so the change takes hold
    /// immediately. Requires the app to run elevated; the text shaping lives in <see cref="HostsContent"/>.
    /// </summary>
    public sealed class HostsFileBlocker : IFocusBlocker
    {
        private readonly string hostsPath;
        private readonly Func<IReadOnlyList<string>> domains;

        public HostsFileBlocker(Func<IReadOnlyList<string>> domains)
            : this(DefaultHostsPath(), domains)
        {
        }

        public HostsFileBlocker(string hostsPath, Func<IReadOnlyList<string>> domains)
        {
            this.hostsPath = hostsPath;
            this.domains = domains;
        }

        public void Block()
        {
            IReadOnlyList<string> blockedDomains = domains();
            if (blockedDomains.Count == 0)
            {
                return;
            }

            Rewrite(existing => HostsContent.WithBlock(existing, blockedDomains));
        }

        public void Unblock()
        {
            Rewrite(HostsContent.WithoutBlock);
        }

        private void Rewrite(Func<string, string> transform)
        {
            string existing = File.Exists(hostsPath) ? File.ReadAllText(hostsPath) : string.Empty;
            string updated = transform(existing);
            if (updated == existing)
            {
                return;
            }

            File.WriteAllText(hostsPath, updated);
            FlushDns();
        }

        private static void FlushDns()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo("ipconfig", "/flushdns")
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };

            try
            {
                Process.Start(startInfo)?.WaitForExit();
            }
            catch
            {
                // A stale DNS cache only delays the block slightly; not worth surfacing.
            }
        }

        private static string DefaultHostsPath()
        {
            string system32 = Environment.GetFolderPath(Environment.SpecialFolder.System);
            return Path.Combine(system32, "drivers", "etc", "hosts");
        }
    }
}
