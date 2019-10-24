using System;

namespace Ovicus.ShowMyHomework.Auth.Extensions
{
    public static class AuthTokenExtensions
    {
        internal static bool IsExpired(this AuthToken token)
        {
            var expiresAt = DateTimeOffset.FromUnixTimeSeconds(token.CreatedAtUnixTimestampSeconds).AddSeconds(token.ExpiresInSeconds);
            // Take 60 seconds to get the request processed at the server and avoid the token hit the server already expired
            return expiresAt.AddSeconds(-60).CompareTo(DateTimeOffset.UtcNow) < 0;
        }
    }
}
