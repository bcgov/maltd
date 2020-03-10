using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BcGov.Malt.Web.Models.SharePoint
{

    public class ContextWebInformation
    {
        [JsonPropertyName("d")] 
        public ResponseData ResponseData { get; set; }
    }

    public class ResponseData
    {
        [JsonPropertyName("GetContextWebInformation")]
        public Getcontextwebinformation GetContextWebInformation { get; set; }
    }

    public class Getcontextwebinformation
    {
        [JsonIgnore]
        public ResponseMetadata Responsemetadata { get; set; }
        [JsonPropertyName("FormDigestTimeoutSeconds")]
        public int FormDigestTimeoutSeconds { get; set; }
        [JsonPropertyName("FormDigestValue")]
        public string FormDigestValue { get; set; }
        [JsonIgnore]
        public string LibraryVersion { get; set; }
        [JsonIgnore]
        public string SiteFullUrl { get; set; }
        [JsonIgnore]
        public Supportedschemaversions SupportedSchemaVersions { get; set; }
        [JsonIgnore]
        public string WebFullUrl { get; set; }
    }

    public class ResponseMetadata
    {
        public string type { get; set; }
    }

    public class Supportedschemaversions
    {
        public __MetadataSchemaVersions __metadata { get; set; }
        public string[] results { get; set; }
    }

    public class __MetadataSchemaVersions
    {
        public string type { get; set; }
    }



  

    

}
