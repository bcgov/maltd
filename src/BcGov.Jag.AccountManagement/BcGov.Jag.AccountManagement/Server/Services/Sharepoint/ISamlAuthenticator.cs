﻿using System.Net;

namespace BcGov.Jag.AccountManagement.Server.Services.Sharepoint;

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
    Task<string> GetStsSamlTokenAsync(string relyingParty, string username, string password, string stsUrl, bool cached = true);

    /// <summary>
    /// Gets the Sharepoint federated authentication cookie.
    /// </summary>
    /// <param name="samlServerUri">The SAML server URI.</param>
    /// <param name="samlToken">The SAML token.</param>
    /// <param name="client">The <see cref="HttpClient"/> to make the request on.</param>
    /// <param name="cookieContainer">The target cookie container.</param>
    /// <param name="apiGatewayHost"></param>
    /// <param name="apiGatewayPolicy"></param>
    /// <returns></returns>
    Task GetSharepointFedAuthCookieAsync(Uri samlServerUri, string samlToken, HttpClient client, CookieContainer cookieContainer, string apiGatewayHost, string apiGatewayPolicy);
}

