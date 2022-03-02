using System.Text.Json.Serialization;

namespace BcGov.Jag.AccountManagement.Server.Models.SharePoint;

public class UserIdInfo
{
    [JsonPropertyName("NameId")]
    public string NameId { get; set; }

    [JsonPropertyName("NameIdIssuer")]
    public string NameIdIssuer { get; set; }
}
