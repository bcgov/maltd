using System;
using System.Text.Json.Serialization;

namespace BcGov.Malt.Web.Models.Authorization
{
    public class Token
    {
        /// <summary>
        /// The function that returns the current UTC date and time. Overriden for tests.
        /// </summary>
        private readonly Func<DateTimeOffset> _utcNow;

        public Token() : this(() => DateTimeOffset.UtcNow)
        {
        }

        internal Token(Func<DateTimeOffset> utcNow)
        {
            _utcNow = utcNow ?? throw new ArgumentNullException(nameof(utcNow));
            CreatedAtUtc = _utcNow();
        }

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
        /// Gets when the access token expires in UTC.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset AccessTokenExpiresAtUtc => CreatedAtUtc.AddSeconds(ExpiresIn);

        /// <summary>
        /// Gets when the refresh token expires in UTC.
        /// </summary>CreatedAtUtc
        [JsonIgnore]
        public DateTimeOffset RefreshTokenExpiresAtUtc => CreatedAtUtc.AddSeconds(RefreshTokenExpiresIn);

        /// <summary>
        /// Gets a value indicating whether the access token is expired.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is expired; otherwise, <c>false</c>.
        /// </value>
        [JsonIgnore]
        public bool IsAccessTokenExpired => AccessTokenExpiresAtUtc <= _utcNow();

        /// <summary>
        /// Gets when the token was created
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset CreatedAtUtc { get; }
    }
}
