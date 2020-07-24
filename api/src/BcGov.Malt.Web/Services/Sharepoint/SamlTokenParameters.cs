using System;

namespace BcGov.Malt.Web.Services.Sharepoint
{
    public class SamlTokenParameters
    {
        public SamlTokenParameters(string relyingParty, string username, string stsUrl)
        {
            RelyingParty = relyingParty ?? string.Empty;
            Username = username ?? string.Empty;
            StsUrl = stsUrl ?? string.Empty;
        }

        public string RelyingParty { get; }

        public string Username { get; }

        public string StsUrl { get; }

        public override int GetHashCode()
        {
            return Tuple.Create(RelyingParty, Username, StsUrl).GetHashCode();
        }
    }
}
