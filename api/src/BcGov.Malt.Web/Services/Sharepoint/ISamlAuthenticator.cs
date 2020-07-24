using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace BcGov.Malt.Web.Services.Sharepoint
{
    public interface ISamlAuthenticator
    {
        /// <summary>
        /// Gets the XML RequestSecurityTokenResponse from the ADFS Secure Token Service (STS).
        /// </summary>
        /// <param name="relyingParty"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="stsUrl"></param>
        /// <param name="cached">if <c>true</c>, will try to use cached token, otherwise a request will be made to real token service.</param>
        /// <returns></returns>
        Task<string> GetStsSamlToken(string relyingParty, string username, string password, string stsUrl, bool cached = true);

        Task GetSharepointFedAuthCookie(Uri samlServerUri, string samlToken, HttpClient client, CookieContainer cookieContainer);
    }
}