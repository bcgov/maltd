using System;
using System.Text.Json.Serialization;

namespace BcGov.Malt.Web.Models.Authorization
{
    public class Token
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("resource")]
        public string Resource { get; set; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonPropertyName("refresh_token_expires_in")]
        public int RefreshTokenExpiresIn { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; }

        [JsonPropertyName("id_token")]
        public string IdToken { get; set; }

        /// <summary>
        /// Gets or sets when the access token expires in UTC.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset ExpiresAtUtc { get; set; }

        /// <summary>
        /// Gets or sets when the refresh token expires in UTC.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset RefreshTokenExpiresAtUtc { get; set; }

        public bool IsExpired => ExpiresAtUtc < DateTimeOffset.UtcNow;
    }
}
