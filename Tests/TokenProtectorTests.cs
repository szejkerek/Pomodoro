using Pomodoro.Services;
using Xunit;

namespace Pomodoro.Tests
{
    public class TokenProtectorTests
    {
        [Fact]
        public void A_protected_token_round_trips_back_to_plaintext()
        {
            string token = "abc123-very-secret-token";

            string stored = TokenProtector.Protect(token);

            Assert.NotEqual(token, stored);
            Assert.Equal(token, TokenProtector.Unprotect(stored));
        }

        [Fact]
        public void Unprotect_returns_legacy_plaintext_unchanged()
        {
            Assert.Equal("legacy-plaintext-token", TokenProtector.Unprotect("legacy-plaintext-token"));
        }

        [Fact]
        public void An_empty_token_stays_empty()
        {
            Assert.Equal("", TokenProtector.Protect(""));
            Assert.Equal("", TokenProtector.Unprotect(""));
        }
    }
}
