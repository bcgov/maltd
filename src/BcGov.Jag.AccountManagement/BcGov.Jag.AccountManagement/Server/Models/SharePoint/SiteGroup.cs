using System.Text.Json.Serialization;

namespace BcGov.Jag.AccountManagement.Server.Models.SharePoint;

public class SiteGroup
{
    [JsonPropertyName("Id")]
    public int Id { get; set; }

    [JsonPropertyName("Title")]
    public string Title { get; set; }
}
