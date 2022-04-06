using System.Text.Json.Serialization;

namespace BcGov.Jag.AccountManagement.Server.Models.Dynamics;


public class Role
{
    [JsonPropertyName("roleid")]
    public Guid RoleId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("businessunitid")]
    public BusinessUnit BusinessUnit { get; set; }

}
