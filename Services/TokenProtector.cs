using System.Runtime.InteropServices;
using System.Text;

namespace Pomodoro.Services
{
    /// <summary>
    /// Encrypts secrets (API tokens) at rest with Windows DPAPI, current-user scope, so a token sitting
    /// in settings.json can't be read by another user or off the machine. Uses crypt32 directly to keep
    /// the app free of NuGet dependencies. Protected values carry a marker prefix; <see cref="Unprotect"/>
    /// returns anything without it unchanged, so tokens written before encryption existed still load.
    /// </summary>
    public static class TokenProtector
    {
        private const string Marker = "DPAPI:";
        private const int CryptProtectUiForbidden = 0x1;

        public static string Protect(string plaintext)
        {
            if (plaintext.Length == 0)
            {
                return string.Empty;
            }

            byte[] encrypted = Transform(Encoding.UTF8.GetBytes(plaintext), protect: true);
            return Marker + Convert.ToBase64String(encrypted);
        }

        public static string Unprotect(string stored)
        {
            if (stored.StartsWith(Marker, StringComparison.Ordinal) == false)
            {
                return stored;
            }

            byte[] cipher = Convert.FromBase64String(stored.Substring(Marker.Length));
            return Encoding.UTF8.GetString(Transform(cipher, protect: false));
        }

        private static byte[] Transform(byte[] data, bool protect)
        {
            DataBlob input = default;
            DataBlob output = default;
            try
            {
                input.Size = data.Length;
                input.Data = Marshal.AllocHGlobal(data.Length);
                Marshal.Copy(data, 0, input.Data, data.Length);

                bool ok = protect
                    ? CryptProtectData(ref input, null, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, CryptProtectUiForbidden, ref output)
                    : CryptUnprotectData(ref input, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, CryptProtectUiForbidden, ref output);
                if (ok == false)
                {
                    throw new InvalidOperationException("Windows DPAPI call failed.");
                }

                byte[] result = new byte[output.Size];
                Marshal.Copy(output.Data, result, 0, output.Size);
                return result;
            }
            finally
            {
                if (input.Data != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(input.Data);
                }

                if (output.Data != IntPtr.Zero)
                {
                    LocalFree(output.Data);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DataBlob
        {
            public int Size;
            public IntPtr Data;
        }

        [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CryptProtectData(
            ref DataBlob input, string? description, IntPtr entropy, IntPtr reserved, IntPtr prompt, int flags, ref DataBlob output);

        [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CryptUnprotectData(
            ref DataBlob input, IntPtr description, IntPtr entropy, IntPtr reserved, IntPtr prompt, int flags, ref DataBlob output);

        [DllImport("kernel32.dll")]
        private static extern IntPtr LocalFree(IntPtr handle);
    }
}
