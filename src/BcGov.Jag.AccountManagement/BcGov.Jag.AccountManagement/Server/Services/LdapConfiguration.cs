using System.ComponentModel.DataAnnotations;

namespace BcGov.Jag.AccountManagement.Server.Services;

internal class LdapConfiguration
{
    public const string Section = "LDAP";

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    [Required]
    public string Server { get; set; }
    [Required]
    public string DistinguishedName { get; set; }
    [Required]
    public string Username { get; set; }
    [Required]
    public string Password { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}