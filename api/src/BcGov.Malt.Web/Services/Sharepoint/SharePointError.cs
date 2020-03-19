using System.Text.Json.Serialization;

namespace BcGov.Malt.Web.Services.Sharepoint
{
    public class SharePointError
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("message")]
        public SharePointErrorMessage Message { get; set; }
    }
}
