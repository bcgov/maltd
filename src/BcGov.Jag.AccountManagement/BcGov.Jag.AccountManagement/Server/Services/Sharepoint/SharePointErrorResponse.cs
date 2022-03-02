using System.Text.Json.Serialization;

namespace BcGov.Jag.AccountManagement.Server.Services.Sharepoint;

/// <summary>
/// SharePoint returns error with this format
/// {"error":{"code":"-2146232832, Microsoft.SharePoint.SPException","message":{"lang":"en-US","value":"The parameter Name cannot be empty or bigger than 255 characters."}}}
/// </summary>
public class SharePointErrorResponse
{
    [JsonPropertyName("error")]
    public SharePointError Error { get; set; }
}
