using System.Text.Json.Serialization;

namespace BcGov.Malt.Web.Models.SharePoint
{
    public class SiteGroup
    {
        [JsonPropertyName("d")]
        public SiteGroupData SiteGroupdata { get; set; }
    }

    public class SiteGroupData
    {
        [JsonPropertyName("__metadata")] 
        public SiteGroupMetadata SiteGroupmetadata { get; set; }

        [JsonPropertyName("Id")]
        public int Id { get; set; }
    }

    public class SiteGroupMetadata
    {
        [JsonPropertyName("id")]
        public string id { get; set; }

        [JsonPropertyName("uri")]
        public string uri { get; set; }

        [JsonPropertyName("type")]
        public string type { get; set; }
    }





}
