using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BcGov.Malt.Web.Services
{
    public abstract class TokenCache<TKey, TToken> : ITokenCache<TKey, TToken>
        where TKey : class
        where TToken : class
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<TokenCache<TKey, TToken>> _logger;
        private readonly Func<DateTimeOffset> _utcNow = () => DateTimeOffset.UtcNow;
        private readonly string _cachePrefix;

        /// <summary>
        /// The expiration buffer. We subtract this amount of time from the user's supplied expiration date
        /// to ensure we dont return an expired token due to time drift on the servers.
        /// </summary>
        private readonly TimeSpan _expirationBuffer = TimeSpan.FromMinutes(1);

        protected TokenCache(IMemoryCache memoryCache, ILogger<TokenCache<TKey, TToken>> logger)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // To ensure each specific implementation does not trash other implementation items
            // in the shared cache. This prefix is added to each computed cache key before it
            // is hashed.
            _cachePrefix = typeof(TKey).FullName + "-" + typeof(TToken).FullName + "-";
        }

        public TToken GetToken(TKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            // prefix each cache with the key's 
            string cacheKey = GetHash(_cachePrefix + GetCacheKey(key));
            _logger.LogTrace("Getting token using {CacheKey}", cacheKey);

            if (_memoryCache.TryGetValue(cacheKey, out TToken token))
            {
                _logger.LogTrace("Cached token found");
                return token;
            }

            _logger.LogTrace("Cached token not found");
            return null;
        }

        public void SaveToken(TKey key, TToken token, DateTimeOffset tokenExpiresAtUtc)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (token == null) throw new ArgumentNullException(nameof(token));

            _logger.LogDebug("Subtracting {Time} from token expiration date to account for time drift between servers", _expirationBuffer);
            tokenExpiresAtUtc = tokenExpiresAtUtc.Subtract(_expirationBuffer);

            DateTimeOffset now = _utcNow();

            if (tokenExpiresAtUtc <= now)
            {
                _logger.LogDebug("Token is already expired, not adding to cache. The current time is {Now} and the token expired at {ExpiresAtUtc}", 
                    now, 
                    tokenExpiresAtUtc);
                return; // token is already expired
            }

            string cacheKey = GetHash(_cachePrefix + GetCacheKey(key));
            _logger.LogTrace("Caching token using cache key {CacheKey} until {ExpiresAtUtc}", cacheKey, tokenExpiresAtUtc);

            _memoryCache.Set(cacheKey, token, tokenExpiresAtUtc);
        }

        /// <summary>
        /// Gets the cache key for this entry.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        protected abstract string GetCacheKey(TKey key);

        private static string GetHash(string value)
        {
#pragma warning disable CA5350 // a weak cryptographic algorithm SHA1
            // SHA1 should be fine as we are not using this value as a password hash
            using HashAlgorithm hashAlgorithm = SHA1.Create();
#pragma warning restore CA5350

            var byteArray = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(value));

            StringBuilder hex = new StringBuilder(byteArray.Length * 2);
            foreach (byte b in byteArray)
            {
                hex.AppendFormat(CultureInfo.InvariantCulture, "{0:x2}", b);
            }

            return hex.ToString();
        }

    }
}
