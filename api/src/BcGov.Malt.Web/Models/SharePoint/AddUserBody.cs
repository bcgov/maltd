using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BcGov.Malt.Web.Models.SharePoint
{
    public class AddUserBody
    {
        [JsonPropertyName("__metadata")] 
        public AddUserBodyMetadata AddUserBodymetadata { get; set; }
        [JsonPropertyName("LoginName")]
        public string LoginName { get; set; }
    }

    public class AddUserBodyMetadata
    {
        [JsonPropertyName("type")]
        public string type { get; set; }
    }
}
