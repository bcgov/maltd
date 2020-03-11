using System.Text.Json.Serialization;

namespace BcGov.Malt.Web.Models.SharePoint
{
    public class ContextWebInformation
    {
        [JsonPropertyName("FormDigestTimeoutSeconds")]
        public int FormDigestTimeoutSeconds { get; set; }

        [JsonPropertyName("FormDigestValue")]
        public string FormDigestValue { get; set; }
    }
}
