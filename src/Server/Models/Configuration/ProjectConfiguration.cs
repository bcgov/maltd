namespace BcGov.Jag.AccountManagement.Server.Models.Configuration;

/// <summary>
/// Contains the configuration information regarding projects.
/// The <see cref="Project"/> has overlap with this type but does not include 
/// credentials for the resources.
/// </summary>
public class ProjectConfiguration
{
    /// <summary>
    /// Gets or sets the name of the project.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the resources in this project.
    /// </summary>
    /// <value>
    /// The resources.
    /// </value>
    public List<ProjectResource> Resources { get; set; }
}
