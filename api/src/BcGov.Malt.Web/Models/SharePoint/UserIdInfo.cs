using System.Text.Json.Serialization;

namespace BcGov.Malt.Web.Models.SharePoint
{
    public class UserIdInfo
    {
        [JsonPropertyName("NameId")]
        public string NameId { get; set; }

        [JsonPropertyName("NameIdIssuer")]
        public string NameIdIssuer { get; set; }
    }
}
