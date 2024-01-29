using System.Text.Json.Serialization;

namespace BcGov.Jag.AccountManagement.Server.Models.SharePoint;

public class ContextWebInformationData
{
    [JsonPropertyName("GetContextWebInformation")]
    public ContextWebInformation ContextWebInformation { get; set; }
}
