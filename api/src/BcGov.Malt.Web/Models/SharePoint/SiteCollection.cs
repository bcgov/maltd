using System.Text.Json.Serialization;

namespace BcGov.Malt.Web.Models.SharePoint
{

    public class SiteCollection
    {
        [JsonPropertyName("d")]
        public SiteCollectionData Data { get; set; }
    }

    public class SiteCollectionData
    {
        [JsonPropertyName("results")]
        public SiteCollectionResult[] Results { get; set; }
    }

    public class SiteCollectionResult
    {
        [JsonIgnore]
        public __Metadata __metadata { get; set; }

        [JsonIgnore]
        public Owner Owner { get; set; }

        [JsonIgnore]
        public Users Users { get; set; }

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

        [JsonIgnore]
        public bool AllowMembersEditMembership { get; set; }

        [JsonIgnore]
        public bool AllowRequestToJoinLeave { get; set; }

        [JsonIgnore]
        public bool AutoAcceptRequestToJoinLeave { get; set; }

        [JsonIgnore]
        public string Description { get; set; }

        [JsonIgnore]
        public bool OnlyAllowMembersViewMembership { get; set; }

        [JsonIgnore]
        public string OwnerTitle { get; set; }

        [JsonIgnore]
        public string RequestToJoinLeaveEmailSetting { get; set; }
    }

    public class __Metadata
    {
        [JsonPropertyName("id")]
        public string id { get; set; }

        [JsonPropertyName("uri")]
        public string uri { get; set; }

        [JsonPropertyName("type")]
        public string type { get; set; }
    }

    public class Owner
    {
        [JsonIgnore]
        public SiteCollectionDeferred __deferred { get; set; }
    }

    public class SiteCollectionDeferred
    {
        [JsonIgnore]
        public string uri { get; set; }
    }

    public class Users
    {
        [JsonIgnore]
        public __Deferred1 __deferred { get; set; }
    }

    public class __Deferred1
    {
        [JsonIgnore]
        public string uri { get; set; }
    }

}
