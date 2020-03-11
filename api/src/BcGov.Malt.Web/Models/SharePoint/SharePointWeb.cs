using System.Text.Json.Serialization;

namespace BcGov.Malt.Web.Models.SharePoint
{
    public class SharePointWeb
    {
        [JsonPropertyName("ServerRelativeUrl")]
        public string ServerRelativeUrl { get; set; }

        [JsonPropertyName("Title")]
        public string Title { get; set; }
    }
}