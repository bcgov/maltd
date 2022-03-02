using System.Text.Json.Serialization;

namespace BcGov.Jag.AccountManagement.Server.Services.Sharepoint;

public class SharePointError
{
    [JsonPropertyName("code")]
    public string Code { get; set; }

    [JsonPropertyName("message")]
    public SharePointErrorMessage Message { get; set; }
}
