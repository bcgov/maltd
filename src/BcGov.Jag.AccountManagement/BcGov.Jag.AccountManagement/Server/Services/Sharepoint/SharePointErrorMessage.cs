using System.Text.Json.Serialization;

namespace BcGov.Jag.AccountManagement.Server.Services.Sharepoint;

public class SharePointErrorMessage
{
    [JsonPropertyName("lang")]
    public string Language { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }

}
