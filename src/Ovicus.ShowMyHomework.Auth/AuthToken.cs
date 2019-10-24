using Newtonsoft.Json;

namespace Ovicus.ShowMyHomework.Auth
{
    internal class AuthToken
    {
        [JsonProperty("smhw_token")]
        public string AccessToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresInSeconds { get; set; }

        [JsonProperty("created_at")]
        public long CreatedAtUnixTimestampSeconds { get; set; }

        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("user_type")]
        public string UserType { get; set; }

        [JsonProperty("school_id")]
        public string SchoolId { get; set; }
    }
}
