using System.Text.Json.Serialization;

namespace BcGov.Malt.Web.Models.SharePoint
{

    public class ContextWebInformationData
    {
        [JsonPropertyName("GetContextWebInformation")]
        public ContextWebInformation ContextWebInformation { get; set; }
    }
}
