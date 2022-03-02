using System.Text.Json.Serialization;

namespace BcGov.Jag.AccountManagement.Server.Models;

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
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary>
    /// The name of the project
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the resources.
    /// </summary>
    /// <value>
    /// The resources.
    /// </value>
    [JsonPropertyName("resources")]
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
    [JsonPropertyName("type")]
    public string Type { get; set; }

    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    /// <value>
    /// The status can be: member, not-member, error
    /// </value>
    [JsonPropertyName("status")]
    public string Status { get; set; }

    /// <summary>
    /// Gets or sets the message related to this status.
    /// Will be used in the case the status is error.
    /// This message may be displayed to the user.
    /// </summary>
    [JsonPropertyName("message")]
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
    [JsonPropertyName("username")]
    public string Username { get; set; }
    
    [JsonPropertyName("access")]
    public List<ProjectResourceStatus> Access { get; set; }
}

public class ProjectAccess
{
    /// <summary>
    /// The id of the project
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary>
    /// The name of the project
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("users")]
    public List<UserAccess> Users { get; set; }
}
