﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BcGov.Malt.Web.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Novell.Directory.Ldap;

namespace BcGov.Malt.Web.Services
{
    public class LdapUserSearchService : IUserSearchService
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

        private readonly IConfiguration _configuration;
        private readonly ILogger<LdapUserSearchService> _logger;

        public LdapUserSearchService(IConfiguration configuration, ILogger<LdapUserSearchService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<User> SearchAsync(string samAccountName)
        {
            return SearchForAsync(samAccountName, LdapSearchAttributes, MapSearchResult);
        }

        private Task<T> SearchForAsync<T>(string query, string[] attributes, Func<LdapEntry, T> mappingFunc) where T : class
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

            using LdapConnection connection = new LdapConnection();
            connection.Connect(configuration.Server, 389);
            connection.Bind(configuration.Username, configuration.Password);

            var searchResults = connection.Search(
                configuration.DistinguishedName,
                LdapConnection.ScopeSub,
                $"(&(objectCategory=person)(objectClass=user)(sAMAccountName={query}))",
                attributes, 
                false);

            if (searchResults.HasMore())
            {
                LdapEntry entry = searchResults.Next();
                return Task.FromResult(mappingFunc(entry));
            }

            return Task.FromResult((T)null); // not found

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
                string joinedSettings = string.Join(", ", missingSettings.ToArray());
                throw new ConfigurationErrorsException("Missing LDAP configuration settings: " + joinedSettings + ".");
            }

            return configuration;
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


    public class LdapConfiguration
    {
        public string Server { get; set; }
        public string DistinguishedName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
