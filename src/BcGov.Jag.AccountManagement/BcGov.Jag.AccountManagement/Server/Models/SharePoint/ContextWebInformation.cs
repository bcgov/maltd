using System.Text.Json.Serialization;

namespace BcGov.Jag.AccountManagement.Server.Models.SharePoint;

public class ContextWebInformation
{
    [JsonPropertyName("FormDigestTimeoutSeconds")]
    public int FormDigestTimeoutSeconds { get; set; }

    [JsonPropertyName("FormDigestValue")]
    public string FormDigestValue { get; set; }
}
