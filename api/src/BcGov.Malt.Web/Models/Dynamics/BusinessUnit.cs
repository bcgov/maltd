using System;
using System.Text.Json.Serialization;

namespace BcGov.Malt.Web.Models.Dynamics
{
    public class BusinessUnit
    {
        [JsonPropertyName("businessunitid")]
        public Guid? BusinessUnitId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("isdisabled")]
        public bool? IsDisabled { get; set; }

        [JsonPropertyName("parentbusinessunitid")]
        public BusinessUnit ParentBusinessUnit { get; set; }
    }
}
