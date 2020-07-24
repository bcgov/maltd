using System;

namespace BcGov.Malt.Web.Models
{
    public static class IDIR
    {
        /// <summary>
        /// The Active Directory domain name for IDIR.
        /// </summary>
        private const string DomainName = "IDIR";

        /// <summary>
        /// Gets the domain qualified format of the IDIR, ie IDIR\username
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns></returns>
        public static string Logon(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            }

            return DomainName + "\\" + username.Trim().ToUpperInvariant();
        }

        /// <summary>
        /// Gets the username portion of the logon name.
        /// </summary>
        /// <param name="login">The login.</param>
        /// <returns></returns>
        public static string Username(string login)
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                throw new ArgumentException("Login cannot be null or empty", nameof(login));
            }

            if (!login.StartsWith(DomainName + "\\", StringComparison.OrdinalIgnoreCase))
            {
                throw new FormatException($"Login must start with {DomainName}\\");
            }

            return login.Substring($"{DomainName}\\".Length).Trim().ToUpperInvariant();
        }

    }
}
