﻿using System.Text;
using BcGov.Jag.AccountManagement.Server.Models.Authorization;
using BcGov.Jag.AccountManagement.Server.Models.Configuration;
using Microsoft.Extensions.Caching.Memory;

namespace BcGov.Jag.AccountManagement.Server.Services;

public class OAuthTokenCache : TokenCache<OAuthOptions, Token>
{
    public OAuthTokenCache(IMemoryCache memoryCache, ILogger<OAuthTokenCache> logger) : base(memoryCache, logger)
    {
    }

    protected override string GetCacheKey(OAuthOptions configuration)
    {
        StringBuilder buffer = new StringBuilder();

        // create a cache key based on all the parameters
        buffer.Append(configuration.AuthorizationUri);
        buffer.Append('|');
        buffer.Append(configuration.Resource);
        buffer.Append('|');
        buffer.Append(configuration.ClientId);
        buffer.Append('|');
        buffer.Append(configuration.ClientSecret);
        buffer.Append('|');
        buffer.Append(configuration.Username);
        buffer.Append('|');
        buffer.Append(configuration.Password);
        buffer.Append('|');

        return buffer.ToString();
    }
}
