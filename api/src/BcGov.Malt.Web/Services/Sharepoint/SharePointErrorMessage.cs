using System.Text.Json.Serialization;

namespace BcGov.Malt.Web.Services.Sharepoint
{
    public class SharePointErrorMessage
    {
        [JsonPropertyName("lang")]
        public string Language { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }

    }
}
