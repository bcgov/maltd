using System.Text.Json.Serialization;

namespace BcGov.Malt.Web.Models.SharePoint
{
    public class AddUserRequest
    {
        [JsonPropertyName("__metadata")] 
        public AddUserMetadata Metadata { get; set; }

        [JsonPropertyName("LoginName")]
        public string LoginName { get; set; }

        public AddUserRequest(string loginName)
        {
            Metadata = new AddUserMetadata {Type = "SP.User"};
            LoginName = loginName;
        }

        public class AddUserMetadata
        {
            [JsonPropertyName("type")]
            public string Type { get; set; }
        }
    }
}
