using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;

namespace BcGov.Malt.Web.Models.Dynamics
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class SystemUser
    {
        [JsonPropertyName("systemuserid")]
        public Guid SystemUserId { get; set; }

        [JsonPropertyName("firstname")]
        public string Firstname { get; set; }

        [JsonPropertyName("lastname")]
        public string Lastname { get; set; }

        [JsonPropertyName("isdisabled")]
        public bool? IsDisabled { get; set; }

        [JsonPropertyName("disabledreason")]
        public string DisabledReason { get; set; }

        [JsonPropertyName("domainname")]
        public string DomainName { get; set; }

        [JsonPropertyName("businessunitid")]
        public BusinessUnit BusinessUnit { get; set; }
        
        [JsonPropertyName("internalemailaddress")]
        public string InternalEMailAddress { get; set; }

        [JsonIgnore]
        private string DebuggerDisplay
        {
            get
            {
                StringBuilder buffer = new StringBuilder();

                // try format Last, First (disabled)
                if (!string.IsNullOrEmpty(Lastname))
                {
                    buffer.Append(Lastname);
                }

                if (!string.IsNullOrEmpty(Firstname))
                {
                    if (buffer.Length != 0)
                    {
                        buffer.Append(", ");
                    }

                    buffer.Append(Firstname);
                }

                if (!string.IsNullOrEmpty(DomainName))
                {
                    if (buffer.Length != 0)
                    {
                        buffer.Append(" - ");
                    }

                    buffer.Append(DomainName);
                }

                if (IsDisabled.HasValue && IsDisabled.Value)
                {
                    if (buffer.Length != 0)
                    {
                        buffer.Append(" ");
                    }

                    buffer.Append("(disabled)");
                }

                if (buffer.Length != 0)
                {
                    return buffer.ToString();
                }

                return SystemUserId.ToString("n", CultureInfo.InvariantCulture);

            }
        }
    }
}
