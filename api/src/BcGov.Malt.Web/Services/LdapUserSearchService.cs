using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BcGov.Malt.Web.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BcGov.Malt.Web.Services
{
    public class LdapUserSearchService : IUserSearchService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<LdapUserSearchService> _logger;

        public LdapUserSearchService(IConfiguration configuration, ILogger<LdapUserSearchService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<User> SearchAsync(string samAccountName)
        {
            return SearchForAsync(samAccountName, MapSearchResult);
        }


        public Task<string> GetUserPrincipalNameAsync(string samAccountName)
        {
            return SearchForAsync(samAccountName, MapToUpn);
        }

        private async Task<T> SearchForAsync<T>(string query, Func<SearchResultEntry, T> mappingFunc) where T : class
        {
            if (string.IsNullOrEmpty(query))
            {
                throw new ArgumentException("Query cannot be null or empty", nameof(query));
            }

            if (!Regex.IsMatch(query, @"^[a-z][a-z0-9\\_]+$", RegexOptions.IgnoreCase))
            {
                _logger.LogInformation("Invalid characters in query {query}, returning null.", query);
                return null;
            }

            LdapConfiguration configuration = GetLdapConfiguration();

            LdapDirectoryIdentifier directory = new LdapDirectoryIdentifier(configuration.Server);
            NetworkCredential credentials = new NetworkCredential(configuration.Username, configuration.Password);

            SearchRequest searchRequest = CreateSearchRequest(configuration.DistinguishedName, query);

            using LdapConnection connection = new LdapConnection(directory, credentials);
            SearchResultEntry entry = await SendRequestAsync(connection, searchRequest);

            if (entry == null)
            {
                return null;
            }

            return mappingFunc(entry);
        }

        private LdapConfiguration GetLdapConfiguration()
        {
            var section = _configuration.GetSection("LDAP");

            var configuration = new LdapConfiguration
            {
                Server = section["Server"],
                DistinguishedName = section["DistinguishedName"],
                Username = section["Username"],
                Password = section["Password"],
            };

            // TODO: check that all of these are configured, log error if they are not
            List<string> missingSettings = new List<string>();

            if (string.IsNullOrEmpty(configuration.Server)) missingSettings.Add(nameof(configuration.Server));
            if (string.IsNullOrEmpty(configuration.DistinguishedName)) missingSettings.Add(nameof(configuration.DistinguishedName));
            if (string.IsNullOrEmpty(configuration.Username)) missingSettings.Add(nameof(configuration.Username));
            if (string.IsNullOrEmpty(configuration.Password)) missingSettings.Add(nameof(configuration.Password));

            if (missingSettings.Count != 0)
            {
                _logger.LogError("Missing LDAP configuration settings {Settings}", missingSettings);

                string joinedSettings = string.Join(", ", missingSettings.ToArray());
                throw new ConfigurationErrorsException("Missing LDAP configuration settings: " + joinedSettings + ".");
            }

            return configuration;
        }

        private Task<SearchResultEntry> SendRequestAsync(LdapConnection connection, SearchRequest searchRequest)
        {
            // convert Begin/End to async
            // example from https://github.com/dotnet/runtime/issues/28470
            return Task.Factory.FromAsync((callback, state) =>
            {
                return connection.BeginSendRequest(searchRequest, PartialResultProcessing.ReturnPartialResultsAndNotifyCallback, callback, state);
            },
            (asyncresult) =>
            {
                SearchResponse searchResponse = (SearchResponse)connection.EndSendRequest(asyncresult);
                if (searchResponse.ResultCode == ResultCode.Success && searchResponse.Entries.Count != 0)
                {
                    return searchResponse.Entries[0];
                }

                return null;
            },
            null);
        }

        private SearchRequest CreateSearchRequest(string distinguishedName, string username)
        {
            SearchRequest searchRequest = new SearchRequest();
            searchRequest.DistinguishedName = distinguishedName;
            searchRequest.Filter = $"(&(objectCategory=person)(objectClass=user)(sAMAccountName={username}))";

            searchRequest.Attributes.Add("bcgovGUID");          // id
            searchRequest.Attributes.Add("sAMAccountName");     // username
            searchRequest.Attributes.Add("userPrincipalName");  // UPN
            searchRequest.Attributes.Add("sn");                 // lastname
            searchRequest.Attributes.Add("givenName");          // firstname
            searchRequest.Attributes.Add("mail");               // email address

            return searchRequest;
        }

        private User MapSearchResult(SearchResultEntry entry)
        {
            var user = new User
            {
                Id = GetAttributeValue(entry, "bcgovGUID") ?? string.Empty,
                UserName = GetAttributeValue(entry, "sAMAccountName") ?? string.Empty,
                FirstName = GetAttributeValue(entry, "givenName") ?? string.Empty,
                LastName = GetAttributeValue(entry, "sn") ?? string.Empty,
                Email = GetAttributeValue(entry, "mail") ?? string.Empty,
            };

            return user;
        }

        private string MapToUpn(SearchResultEntry entry)
        {
            return GetAttributeValue(entry, "userPrincipalName") ?? string.Empty;
        }

        private string GetAttributeValue(SearchResultEntry entry, string attributeName)
        {
            DirectoryAttribute attribute = entry.Attributes[attributeName];

            object[] values = attribute?.GetValues(typeof(string));

            return values != null && values.Length == 1 ? (string)values[0] : null;
        }
    }


    public class LdapConfiguration
    {
        public string Server { get; set; }
        public string DistinguishedName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
