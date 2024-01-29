using System.Text.Json.Serialization;

namespace BcGov.Jag.AccountManagement.Server.Models.Dynamics;

public class BusinessUnit
{
    [JsonPropertyName("businessunitid")]
    public Guid? BusinessUnitId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("isdisabled")]
    public bool? IsDisabled { get; set; }

    [JsonPropertyName("parentbusinessunitid")]
    public BusinessUnit ParentBusinessUnit { get; set; }
}
