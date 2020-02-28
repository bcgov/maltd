using System;
using System.Security.Cryptography;
using System.Text;
using BcGov.Malt.Web.Models.Authorization;
using BcGov.Malt.Web.Models.Configuration;
using Microsoft.Extensions.Caching.Memory;

namespace BcGov.Malt.Web.Services
{
    public class TokenCache : ITokenCache
    {
        private IMemoryCache _memoryCache;

        public TokenCache(IMemoryCache memoryCache) => _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));

        public Token GetToken(OAuthOptions configuration)
        {
            string key = GetCacheKey(configuration);

            if (_memoryCache.TryGetValue<Token>(key, out Token token))
            {
                return token;
            }

            return null;
        }

        public void SaveToken(OAuthOptions configuration, Token token)
        {
            if (token.IsAccessTokenExpired)
            {
                return;
            }

            string key = GetCacheKey(configuration);

            _memoryCache.Set(key, token, token.AccessTokenExpiresAtUtc);
        }

        private string GetCacheKey(OAuthOptions configuration)
        {
            StringBuilder buffer = new StringBuilder();

            // create a cache key based on all the parameters
            buffer.Append(configuration.AuthorizationUri.ToString());
            buffer.Append('|');
            buffer.Append(configuration.Resource.ToString());
            buffer.Append('|');
            buffer.Append(configuration.ClientId);
            buffer.Append('|');
            buffer.Append(configuration.ClientSecret);
            buffer.Append('|');
            buffer.Append(configuration.Username);
            buffer.Append('|');
            buffer.Append(configuration.Password);
            buffer.Append('|');

            return GetHash(buffer.ToString());
        }


        private static string GetHash(string value)
        {
            // SHA1 should be fine as we are not using this value as a password hash

            using HashAlgorithm hashAlgorithm = SHA1.Create();
            var byteArray = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(value));

            StringBuilder hex = new StringBuilder(byteArray.Length * 2);
            foreach (byte b in byteArray)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            return hex.ToString();
        }
    }
}
