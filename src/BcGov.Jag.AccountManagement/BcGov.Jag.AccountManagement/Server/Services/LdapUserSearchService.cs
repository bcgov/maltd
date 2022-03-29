using System.Text.RegularExpressions;
using Novell.Directory.Ldap;
using BcGov.Jag.AccountManagement.Shared;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using BcGov.Jag.AccountManagement.Server.Infrastructure;

namespace BcGov.Jag.AccountManagement.Server.Services;

internal class LdapUserSearchService : IUserSearchService, IDisposable
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
    private LdapConnection? _ldapConnection = null;
    private int _locked = 0;

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

        using var userSearchActivity = DiagnosticTrace.StartActivity("User Search");
        userSearchActivity?.AddTag("sAMAccountName", query);

        try
        {
            var connection = await GetLdapConnectionAsync();

            ILdapSearchResults? searchResults = null;

            using (var searchActivity = DiagnosticTrace.StartActivity("Active Directory Search"))
            {
                searchResults = await connection.SearchAsync(
                    _configuration.DistinguishedName,
                    LdapConnection.ScopeSub,
                    $"(&(objectCategory=person)(objectClass=user)(sAMAccountName={query}))",
                    attributes,
                    false);
            }

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

    public async Task<ActiveDirectoryUserStatus?> GetAccountStatusAsync(string username, CancellationToken cancellationToken)
    {
        const string UserAccountControlAttribute = "userAccountControl";

        using var userSearchActivity = DiagnosticTrace.StartActivity("Active Directory User Lookup");
        userSearchActivity?.AddTag("sAMAccountName", username);

        var connection = await GetLdapConnectionAsync();

        var searchResults = await connection.SearchAsync(
            _configuration.DistinguishedName,
            LdapConnection.ScopeSub,
            $"(&(objectCategory=person)(objectClass=user)(sAMAccountName={username}))",
            new[] { UserAccountControlAttribute },
            false);

        // there will be zero or one since sAMAccountName must be unique in Active Directory
        await foreach (var entry in searchResults)
        {
            UserAccountControl accountControl = default(UserAccountControl);

            LdapAttributeSet attributeSet = entry.GetAttributeSet();
            if (attributeSet.TryGetValue(UserAccountControlAttribute, out var ldapAttributeValue))
            {
                accountControl = (UserAccountControl) int.Parse(ldapAttributeValue.StringValue);
            }


            return new ActiveDirectoryUserStatus { Username = username, UserAccountControl = accountControl };
        }

        return null; // not found
    }

    private async Task<LdapConnection> GetLdapConnectionAsync()
    {
        if (_ldapConnection is not null)
        {
            return _ldapConnection;
        }

        while (Interlocked.CompareExchange(ref _locked, 1, 0) != 0)
        {
            // spin, we dont hold the lock
            if (_ldapConnection is not null)
            {
                return _ldapConnection;
            }
        }

        Debug.Assert(_locked == 1);

        try
        {
            LdapConnection connection = new LdapConnection();
            using (var connectActivity = DiagnosticTrace.StartActivity("Active Directory Connect"))
            {
                connectActivity?.AddTag("Server", _configuration.Server);
                connectActivity?.AddTag("Port", 389);

                await connection.ConnectAsync(_configuration.Server, 389);
            }

            using (var bindActivity = DiagnosticTrace.StartActivity("Active Directory Bind"))
            {
                await connection.BindAsync(_configuration.Username, _configuration.Password);
            }

            _ldapConnection = connection;

            return connection;
        }
        finally
        {
            _locked = 0;
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

    public void Dispose()
    {
        _ldapConnection?.Dispose();
    }
}


public class ActiveDirectoryUserStatus
{
    public string Username { get; set; }
    public UserAccountControl UserAccountControl;
}

[Flags]
public enum UserAccountControl
{
    AccountDisabled = 0x0002,
    Lockout = 0x0010,
    PasswordExpired = 0x800000,
}