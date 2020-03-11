using System.Text.Json.Serialization;

namespace BcGov.Malt.Web.Models.SharePoint
{

    public class ContextWebInformationData
    {
        [JsonPropertyName("GetContextWebInformation")]
        public ContextWebInformation ContextWebInformation { get; set; }
    }

    public class ContextWebInformation
    {
        [JsonPropertyName("FormDigestTimeoutSeconds")]
        public int FormDigestTimeoutSeconds { get; set; }

        [JsonPropertyName("FormDigestValue")]
        public string FormDigestValue { get; set; }
    }

    public class GetContextWebInformationVerboseResponse : ODataVerboseResponse<ContextWebInformationData>
    {
    }
}
