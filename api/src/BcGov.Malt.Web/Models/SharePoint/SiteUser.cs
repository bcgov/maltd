using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BcGov.Malt.Web.Models.SharePoint
{

    public class SiteUser
    {
        [JsonPropertyName("d")] 
        public SiteUserData SiteUserData { get; set; }
    }

    public class SiteUserData
    {
        [JsonPropertyName("results")] 
        public SiteUserResult[] results { get; set; }
    }

    public class SiteUserResult
    {

        [JsonPropertyName("__metadata")] 
        public SiteUserMetadata __metadata { get; set; }
        [JsonPropertyName("Groups")]
        public SiteUserGroups Groups { get; set; }
        [JsonPropertyName("Id")]
        public int Id { get; set; }
        [JsonPropertyName("IsHiddenInUI")]
        public bool IsHiddenInUI { get; set; }
        [JsonPropertyName("LoginName")]
        public string LoginName { get; set; }
        [JsonPropertyName("Title")]
        public string Title { get; set; }
        [JsonPropertyName("PrincipalType")]
        public int PrincipalType { get; set; }
        [JsonPropertyName("Email")]
        public string Email { get; set; }
        [JsonPropertyName("IsShareByEmailGuestUser")]
        public bool IsShareByEmailGuestUser { get; set; }
        [JsonPropertyName("IsSiteAdmin")]
        public bool IsSiteAdmin { get; set; }
        [JsonPropertyName("UserId")]
        public UseridInSite UserId { get; set; }
    }

    public class SiteUserMetadata
    {
        [JsonPropertyName("id")]
        public string id { get; set; }
        [JsonPropertyName("uri")]
        public string uri { get; set; }
        [JsonPropertyName("type")]
        public string type { get; set; }
    }

    public class SiteUserGroups
    {
        [JsonPropertyName("__deferred")] 
        public SiteUserDeferred __deferred { get; set; }
    }

    public class SiteUserDeferred
    {
        [JsonPropertyName("uri")]
        public string uri { get; set; }
    }

    public class UseridInSite
    {
        [JsonPropertyName("__metadata")]
        public SiteUserMetadata1 __metadata { get; set; }
        [JsonPropertyName("NameId")]
        public string NameId { get; set; }
        [JsonPropertyName("NameIdIssuer")]
        public string NameIdIssuer { get; set; }
    }

    public class SiteUserMetadata1
    {
        [JsonPropertyName("type")]
        public string type { get; set; }
    }

}
