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

    public string LoginName { get; set; }
}

/// <summary>
/// Represents a user with the currently assigned projects
/// </summary>
public class DetailedUser : User
{
    public DetailedUser()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="DetailedUser"/> class.</summary>
    /// <param name="user">The user.</param>
    /// <param name="projects">The projects.</param>
    public DetailedUser(User user, IEnumerable<Project> projects)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        Id = user.Id;
        FirstName = user.FirstName;
        LastName = user.LastName;
        Email = user.Email;
        UserName = user.UserName;
        UserPrincipalName = user.UserPrincipalName;
        Projects = projects != null ? projects.ToArray() : Array.Empty<Project>();
    }

    /// <summary>
    /// The projects the user is assigned to.
    /// </summary>
    public Project[] Projects { get; set; }
}

/// <summary>
/// Represents a project that a user may or may not be granted access to.
/// Contains a list of each resource and if the user is a member or not.
/// </summary>
public class Project
{
    public Project()
    {
    }

    public Project(string id, string name)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("Id cannot be null or empty", nameof(id));
        }

        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Name cannot be null or empty", nameof(name));
        }

        Id = id;
        Name = name;
    }

    /// <summary>
    /// The id of the project
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// The name of the project
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the resources.
    /// </summary>
    /// <value>
    /// The resources.
    /// </value>
    public List<ProjectResourceStatus> Resources { get; set; }
}

public class ProjectResourceStatus
{
    /// <summary>
    /// Gets or sets the type of the resource
    /// </summary>
    /// <value>
    /// The type will be Dynamics or Sharepoint
    /// </value>
    public string Type { get; set; }

    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    /// <value>
    /// The status can be: member, not-member, error
    /// </value>
    public string Status { get; set; }

    /// <summary>
    /// Gets or sets the message related to this status.
    /// Will be used in the case the status is error.
    /// This message may be displayed to the user.
    /// </summary>
    public string Message { get; set; }
}

public static class ProjectResourceStatuses
{
    public static readonly string Member = "member";
    public static readonly string NotMember = "not-member";
    public static readonly string Error = "error";
}

public class UserAccess
{
    public string Username { get; set; }

    public List<ProjectResourceStatus> Access { get; set; }
}

public class ProjectAccess
{
    /// <summary>
    /// The id of the project
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// The name of the project
    /// </summary>
    public string Name { get; set; }

    public List<UserAccess> Users { get; set; }
}


/// <summary>
/// Represents a project that a user can be granted access to.
/// </summary>
public class ProjectDefinition
{
    public ProjectDefinition(string id, string name)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("Id cannot be null or empty", nameof(id));
        }

        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Name cannot be null or empty", nameof(name));
        }

        Id = id;
        Name = name;
    }

    /// <summary>
    /// The id of the project
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// The name of the project
    /// </summary>
    public string Name { get; set; }
}