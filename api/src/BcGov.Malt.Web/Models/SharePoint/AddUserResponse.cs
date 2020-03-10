using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BcGov.Malt.Web.Models.SharePoint
{

    public class AddUserResponse
    {
        [JsonPropertyName("d")]
        public AddUserResponseData ResponseData { get; set; }
    }

    public class AddUserResponseData
    {
        [JsonPropertyName("__metadata")]
        public AddUserResponseMetadata __metadata { get; set; }
        [JsonIgnore]
        public Groups Groups { get; set; }
        [JsonPropertyName("Id")]
        public int Id { get; set; }
        [JsonIgnore]
        public bool IsHiddenInUI { get; set; }
        [JsonPropertyName("LoginName")]
        public string LoginName { get; set; }
        [JsonPropertyName("Title")]
        public string Title { get; set; }
        [JsonPropertyName("PrincipalType")]
        public int PrincipalType { get; set; }
        [JsonPropertyName("Email")]
        public string Email { get; set; }
        [JsonIgnore]
        public bool IsShareByEmailGuestUser { get; set; }
        [JsonPropertyName("IsSiteAdmin")]
        public bool IsSiteAdmin { get; set; }
        [JsonPropertyName("UserId")]
        public Userid UserId { get; set; }
    }

    public class AddUserResponseMetadata
    {
        [JsonPropertyName("id")]
        public string id { get; set; }
        [JsonPropertyName("uri")]
        public string uri { get; set; }
        [JsonPropertyName("type")]
        public string type { get; set; }
    }

    public class Groups
    {
        [JsonPropertyName("__deferred")]
        public __Deferred __deferred { get; set; }
    }

    public class __Deferred
    {
        public string uri { get; set; }
    }

    public class Userid
    {
        [JsonPropertyName("__metadata")]
        public __Metadata1 __metadata { get; set; }
        [JsonPropertyName("NameId")]
        public string NameId { get; set; }
        [JsonPropertyName("NameIdIssuer")]
        public string NameIdIssuer { get; set; }
    }

    public class __Metadata1
    {
        [JsonPropertyName("type")]
        public string type { get; set; }
    }

    public class RemoveUserResponse
    {
        public RemoveUserResponseData d { get; set; }
    }

    public class RemoveUserResponseData
    {
        public object RemoveById { get; set; }
    }

}
