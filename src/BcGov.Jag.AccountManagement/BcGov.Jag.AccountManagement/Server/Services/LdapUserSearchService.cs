using System.Text.RegularExpressions;
using Novell.Directory.Ldap;
using BcGov.Jag.AccountManagement.Shared;
using Microsoft.Extensions.Options;

namespace BcGov.Jag.AccountManagement.Server.Services;

internal class LdapUserSearchService : IUserSearchService
{
    private static readonly string[] LdapSearchAttributes = new[]
    {
        "bcgovGUID",          // id
        "sAMAccountName",     // username
        "sn",                 // lastname
        "givenName",          // firstname
        "mail",                // email address
        "userPrincipalName",  // UPN
    };

    private readonly LdapConfiguration _configuration;
    private readonly ILogger<LdapUserSearchService> _logger;

    public LdapUserSearchService(IOptions<LdapConfiguration> configuration, ILogger<LdapUserSearchService> logger)
    {
        _configuration = configuration?.Value ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<User?> SearchAsync(string samAccountName, CancellationToken cancellationToken)
    {
        User? user = await SearchForAsync(samAccountName, LdapSearchAttributes);
        return user;
    }

    private async Task<User?> SearchForAsync(string query, string[] attributes)
    {
        if (string.IsNullOrEmpty(query))
        {
            throw new ArgumentException("Query cannot be null or empty", nameof(query));
        }

        if (!Regex.IsMatch(query, @"^[a-z][a-z0-9\\_]+$", RegexOptions.IgnoreCase))
        {
            _logger.LogInformation("Invalid characters in query {query}, returning null.", query);
            throw new UserSearchInvalidException($"Invalid characters in query {query}, returning null.");
        }

        try
        {
            using LdapConnection connection = new LdapConnection();
            await connection.ConnectAsync(_configuration.Server, 389);
            await connection.BindAsync(_configuration.Username, _configuration.Password);

            var searchResults = await connection.SearchAsync(
                _configuration.DistinguishedName,
                LdapConnection.ScopeSub,
                $"(&(objectCategory=person)(objectClass=user)(sAMAccountName={query}))",
                attributes,
                false);

            User? user = null; // not found

            // there will be zero or one since sAMAccountName must be unique in Active Directory
            await foreach (var entry in searchResults)
            {
                user = MapSearchResult(entry);
            }
            
            return user; 
        }
        catch (Exception exception)
        {
            // TODO: get more specific exception types thrown and provide better error messages
            _logger.LogError(exception, "Failed to execute user search");
            throw new UserSearchFailedException("Failed to execute user search", exception);
        }
    }

    private User MapSearchResult(LdapEntry entry)
    {
        LdapAttributeSet attributeSet = entry.GetAttributeSet();

        var user = new User
        {
            Id = GetValueOrDefault(attributeSet, "bcgovGUID"),
            UserName = GetValueOrDefault(attributeSet, "sAMAccountName"),
            FirstName = GetValueOrDefault(attributeSet, "givenName"),
            LastName = GetValueOrDefault(attributeSet, "sn"),
            Email = GetValueOrDefault(attributeSet, "mail"),
            UserPrincipalName = GetValueOrDefault(attributeSet, "userPrincipalName")
        };

        return user;
    }

    private string GetValueOrDefault(LdapAttributeSet attributeSet, string name)
    {
        if (attributeSet.TryGetValue(name, out var ldapAttributeValue))
        {
            return ldapAttributeValue.StringValue;
        }

        return string.Empty;
    }
}
