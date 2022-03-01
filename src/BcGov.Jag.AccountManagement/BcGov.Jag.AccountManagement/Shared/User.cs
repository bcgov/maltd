using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BcGov.Jag.AccountManagement.Shared;

/// <summary>
/// Represents a user
/// </summary>
public class User
{
    /// <summary>Gets or sets the identifier.</summary>
    /// <value>The identifier.</value>
    public string? Id { get; set; }

    /// <summary>Gets or sets the name of the user.</summary>
    /// <value>The name of the user.</value>
    public string? UserName { get; set; }

    /// <summary>Gets or sets the first name.</summary>
    /// <value>The first name.</value>
    public string? FirstName { get; set; }

    /// <summary>Gets or sets the last name.</summary>
    /// <value>The last name.</value>
    public string? LastName { get; set; }

    /// <summary>Gets or sets the email.</summary>
    /// <value>The email.</value>
    public string? Email { get; set; }

    /// <summary>Gets or sets the pser principal name (upn).</summary>
    /// <value>The email.</value>
    public string? UserPrincipalName { get; set; }
}
