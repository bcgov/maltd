using System.Diagnostics;
using System.Text.Json.Serialization;

namespace BcGov.Malt.Web.Models.SharePoint
{
    public class LoginUser
    {
        [JsonPropertyName("LoginName")]
        public string LoginName { get; set; }

    }

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class User : LoginUser
    {
        [JsonPropertyName("Id")]
        public int Id { get; set; }

        [JsonPropertyName("Title")]
        public string Title { get; set; }

        [JsonPropertyName("Email")]
        public string Email { get; set; }

        [JsonPropertyName("UserId")]
        public UserIdInfo UserId { get; set; }

        [JsonIgnore]
        private string DebuggerDisplay
        {
            get
            {
                if (!string.IsNullOrEmpty(Title) && !string.IsNullOrEmpty(Email))
                {
                    return Title + " (" + Email + ")";
                }

                return LoginName;
            }
        }
    }
}
