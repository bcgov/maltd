using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.Logging;

namespace BcGov.Malt.Web.Services.Sharepoint
{
    /*
     *  SharePoint Authentication functions.
     *  
     *  The original source of this code was from this project:
     *  
     *  https://github.com/bcgov/jag-lcrb-carla-public/blob/dc80f460796d64aa89ee5c67d9c62e4b9f066036/cllc-interfaces/SharePoint/Authentication.cs
     *  
     *  Changed to use XML libraries to manipulate SOAP requests instead of using string replacements to avoid issues with special reserved XML characters
     *  potentially in passwords.
     */


    public class SamlAuthenticator : ISamlAuthenticator
    {
        private static readonly TimeSpan DefaultHttpClientDataTimeout = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan DefaultHttpClientAuthTimeout = TimeSpan.FromSeconds(15);

        private readonly ITokenCache<SamlTokenParameters, string> _tokenCache;
        private readonly ILogger<SamlAuthenticator> _logger;

        public SamlAuthenticator(ITokenCache<SamlTokenParameters, string> tokenCache, ILogger<SamlAuthenticator> logger)
        {
            _tokenCache = tokenCache ?? throw new ArgumentNullException(nameof(tokenCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the XML RequestSecurityTokenResponse from the ADFS Secure Token Service (STS).
        /// </summary>
        /// <param name="relyingParty"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="stsUrl"></param>
        /// <param name="cached"></param>
        /// <returns></returns>
        public async Task<string> GetStsSamlTokenAsync(string relyingParty, string username, string password, string stsUrl, bool cached = true)
        {
            string securityTokenResponse;

            SamlTokenParameters tokenParameters = new SamlTokenParameters(relyingParty, username, stsUrl);
            if (cached)
            {
                _logger.LogDebug("Checking for cached token");
                securityTokenResponse = _tokenCache.GetToken(tokenParameters);
                if (securityTokenResponse != null)
                {
                    _logger.LogTrace("Returning cached token");
                    return securityTokenResponse;
                }

                _logger.LogTrace("Cached token not found");
            }

            // Makes a request that conforms with the WS-Trust standard to 
            // the Security Token Service to get a SAML security token back 

            // generate the WS-Trust security token request SOAP message 
            string samlSoapRequest = CreateSamlSoapRequest(relyingParty, username, password, stsUrl);
            using var content = new StringContent(samlSoapRequest, Encoding.UTF8, "application/soap+xml");

            using var client = new HttpClient();
            client.Timeout = DefaultHttpClientAuthTimeout;
            var responseMessage = await client.PostAsync(stsUrl, content);

            // A valid response needs to be a SOAP element, Content Type = application/soap+xml
            // Invalid parameters can still return 200 but will be an HTML document

            var responseContent = await responseMessage.Content.ReadAsStringAsync();
            
            if (TryParseXml(responseContent, out XmlDocument soapResponse))
            {
                // <a:Action s:mustUnderstand="1">http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/Issue</a:Action>
                // <a:Action s:mustUnderstand="1">http://www.w3.org/2005/08/addressing/soap/fault</a:Action>
                string action = SelectSoapAction(soapResponse);

                // The response could also be a SOAP Fault
                if (!responseMessage.IsSuccessStatusCode)
                {
                    if (action == "http://www.w3.org/2005/08/addressing/soap/fault")
                    {
                        /*
                         *    <s:Body>
                         *        <s:Fault>
                         *            <s:Code>
                         *                <s:Value>s:Sender</s:Value>
                         *                <s:Subcode>
                         *                    <s:Value>a:DestinationUnreachable</s:Value>
                         *                </s:Subcode>
                         *            </s:Code>
                         *            <s:Reason>
                         *                <s:Text xml:lang="en-US">The message with To 'https://ststest.gov.bc.ca/adfs/services/trust/2005/UsernameMixed2' cannot be processed at the receiver, due to an AddressFilter mismatch at the EndpointDispatcher.  Check that the sender and receiver's EndpointAddresses agree.</s:Text>
                         *            </s:Reason>
                         *        </s:Fault>
                         *    </s:Body>
                         *
                         */
                        XmlNamespaceManager namespaceManager = GetXmlNamespaceManager(soapResponse);

                        var code = GetNodeInnerText(soapResponse, namespaceManager, "/s:Envelope/s:Body/s:Fault/s:Code/s:Value");
                        var subcode = GetNodeInnerText(soapResponse, namespaceManager, "/s:Envelope/s:Body/s:Fault/s:Code/s:Subcode");
                        var reason = GetNodeInnerText(soapResponse, namespaceManager, "/s:Envelope/s:Body/s:Fault/s:Reason/s:Text");

                        _logger.LogWarning("Request for security token resulted in SOAP Fault and was not successful. STSUrl={StsUrl}, RelyingParty={RelyingParty}, Fault={@Fault}",
                            stsUrl,
                            relyingParty,
                            new { Code = code, Subcode = subcode, Reason = reason });

                        throw new SamlAuthenticationException($"Request to {stsUrl} return HTTP Status {responseMessage.StatusCode}", code, subcode, reason);
                    }
                    else
                    {
                        _logger.LogWarning("Request for security token was not successful and was not a SOAP Fault. Returned {HttpStatusCode}, STS Url is {StsUrl} and Relying Party {RelyingParty}",
                            responseMessage.StatusCode,
                            stsUrl,
                            relyingParty);
                    }

                    // TODO: change to better custom exception, read any possible content and log
                    throw new SamlAuthenticationException($"Request to {stsUrl} return HTTP Status {responseMessage.StatusCode}", string.Empty, string.Empty, string.Empty);
                }

                else if (action == "http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/Issue")
                {
                    securityTokenResponse = ExtractSoapBody(soapResponse);

                    // we don't need the lifetime if we are not caching
                    if (cached)
                    {
                        var lifetime = ExtractLifetimeTimestamps(soapResponse);
                        _logger.LogInformation("Security token has {@Lifetime} and will be cached until it expires", lifetime);

                        _tokenCache.SaveToken(tokenParameters, securityTokenResponse, lifetime.Expires);
                    }

                    return securityTokenResponse;
                }
                else
                {
                    // TODO: throw custom exception related to SOAP fault
                    throw new SamlAuthenticationException($"Unexpected SOAP Action : {action}");
                }
            }
            else
            {
                // response could be a HTML error page
                // TODO: throw custom exception related to ADFS error page
                throw new SamlAuthenticationException("Invalid SOAP Response. Content is not valid XML");
            }
        }

        public async Task GetSharepointFedAuthCookieAsync(Uri samlServerUri, string samlToken, HttpClient client, CookieContainer cookieContainer)
        {
            // Single Sign On endpoint to post SAML token too
            var trustUri = new Uri(samlServerUri, "_trust/");

            _logger.LogTrace("Posting request to URI {TrustUri} to obtain FedAuth HTTP Cookie", trustUri);

            // create the body of the POST
            var data = new Dictionary<string, string>
            {
                { "wa", "wsignin1.0" },
                { "wctx", new Uri(samlServerUri, "_layouts/Authenticate.aspx?Source=%2F").ToString() },
                { "wresult", samlToken }
            };

            using var content = new FormUrlEncodedContent(data);

            // save the timeout so we can restore after our request
            var previousTimeout = client.Timeout;
            client.Timeout = DefaultHttpClientAuthTimeout;

            var httpPostResponse = await client.PostAsync(trustUri, content);

            // restore the timeout
            client.Timeout = previousTimeout;

            // the response could be 302 as well
            if (!httpPostResponse.IsSuccessStatusCode && httpPostResponse.StatusCode != HttpStatusCode.Found)
            {
                _logger.LogError("POST request to {TrustUri} was not successful, HttpStatusCode={HttpStatusCode}",
                    trustUri,
                    httpPostResponse.StatusCode);
            }

            // get the schema and authority of the SAML server (ie Sharepoint)
            var wreplyUri = new Uri(samlServerUri.GetLeftPart(UriPartial.Authority));

            // if we are using an API gateway we need to copy the fedAuth
            // cookie to the wreply host.
            if (trustUri != wreplyUri)
            {
                CookieCollection cookies = cookieContainer.GetCookies(trustUri);
                string fedAuthCookieValue = cookies["FedAuth"]?.Value;

                if (!string.IsNullOrEmpty(fedAuthCookieValue))
                {
                    cookieContainer.Add(wreplyUri, new Cookie("FedAuth", fedAuthCookieValue, "/"));
                }
                else
                {
                    _logger.LogInformation("FedAuth cookie not found");
                }
            }
        }

        /// <summary>
        /// Attempts to parse the xml string into an <see cref="XmlDocument"/>.
        /// </summary>
        /// <param name="xml">The potential XML content.</param>
        /// <param name="document">The output <see cref="XmlDocument"/></param>
        /// <returns><c>true</c> if the XML was successfully parse, otherwise <c>false</c>.</returns>
        private static bool TryParseXml(string xml, out XmlDocument document)
        {
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(xml);

                document = xmlDocument;
                return true;
            }
            catch (XmlException)
            {
                document = null;
                return false;
            }
        }

        /// <summary>
        /// Returns the SOAP Action from the header.
        /// </summary>
        private static string SelectSoapAction(XmlDocument document)
        {
            // create a name space manager for the prefixes we need to reference
            XmlNamespaceManager namespaceManager = GetXmlNamespaceManager(document);

            XmlNode actionNode = document.SelectSingleNode("/s:Envelope/s:Header/a:Action", namespaceManager);
            string action = actionNode?.InnerText ?? string.Empty;

            if (string.IsNullOrEmpty(action))
            {
                // TODO: log that the XML did not contain an Action?
            }

            return action;
        }

        /// <summary>
        /// Create a SOAP Envelope message for requesting a SAML token
        /// </summary>
        /// <param name="relyingParty"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="toUrl"></param>
        /// <returns></returns>
        private static string CreateSamlSoapRequest(string relyingParty, string username, string password, string toUrl)
        {
            // XML here uses single quotes to allow us to use a verbatim string literal
            // the final XML will have normal double quotes
            string samlRTString = @"
<s:Envelope xmlns:s='http://www.w3.org/2003/05/soap-envelope' xmlns:a='http://www.w3.org/2005/08/addressing' xmlns:u='http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd'>
  <s:Header>
    <a:Action s:mustUnderstand='1'>http://schemas.xmlsoap.org/ws/2005/02/trust/RST/Issue</a:Action>
    <a:ReplyTo>
      <a:Address>http://www.w3.org/2005/08/addressing/anonymous</a:Address>
    </a:ReplyTo>
    <a:To s:mustUnderstand='1'>[toUrl]</a:To>
    <o:Security s:mustUnderstand='1' xmlns:o='http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd'>
      <o:UsernameToken u:Id='uuid-6a13a244-dac6-42c1-84c5-cbb345b0c4c4-1'>
        <o:Username>[username]</o:Username>
        <o:Password>[password]</o:Password>
      </o:UsernameToken>
    </o:Security>
  </s:Header>
  <s:Body>
    <t:RequestSecurityToken xmlns:t='http://schemas.xmlsoap.org/ws/2005/02/trust'>
      <wsp:AppliesTo xmlns:wsp='http://schemas.xmlsoap.org/ws/2004/09/policy'>
        <a:EndpointReference>
          <a:Address>[url]</a:Address>
        </a:EndpointReference>
      </wsp:AppliesTo>
      <t:KeyType>http://schemas.xmlsoap.org/ws/2005/05/identity/NoProofKey</t:KeyType>
      <t:RequestType>http://schemas.xmlsoap.org/ws/2005/02/trust/Issue</t:RequestType>
      <t:TokenType>urn:oasis:names:tc:SAML:1.0:assertion</t:TokenType>
    </t:RequestSecurityToken>
  </s:Body>
</s:Envelope>";

            // load the xml message and use the XML methods to set element contents
            // this is safer than using string replacements as the password could contain
            // character sequences that are invalid in an XML element
            XmlDocument soapDocument = new XmlDocument();
            soapDocument.LoadXml(samlRTString);

            // create a name space manager for the prefixes we need to reference
            XmlNamespaceManager namespaceManager = GetXmlNamespaceManager(soapDocument);

            // Update correct elements content using XPath. These paths could be simplified
            // but are full path to avoid possible errors 
            soapDocument.SelectSingleNode("/s:Envelope/s:Header/a:To", namespaceManager).InnerText = toUrl;
            soapDocument.SelectSingleNode("/s:Envelope/s:Header/o:Security/o:UsernameToken/o:Username", namespaceManager).InnerText = username;
            soapDocument.SelectSingleNode("/s:Envelope/s:Header/o:Security/o:UsernameToken/o:Password", namespaceManager).InnerText = password;
            soapDocument.SelectSingleNode("/s:Envelope/s:Body/t:RequestSecurityToken/wsp:AppliesTo/a:EndpointReference/a:Address", namespaceManager).InnerText = relyingParty;

            // return the document as a string
            return soapDocument.OuterXml;
        }

        /// <summary>
        /// Returns the inner contents of the SOAP body.
        /// </summary>
        /// <param name="soapDocument"></param>
        /// <returns></returns>
        private static string ExtractSoapBody(XmlDocument soapDocument)
        {
            XmlNamespaceManager namespaceManager = GetXmlNamespaceManager(soapDocument);

            string body = GetNodeInnerXml(soapDocument, namespaceManager, "/s:Envelope/s:Body");
            return body;
        }

        private static RequestSecurityTokenResponseLifetime ExtractLifetimeTimestamps(XmlDocument soapDocument)
        {
            XmlNamespaceManager namespaceManager = GetXmlNamespaceManager(soapDocument);

            /*
             <s:Envelope
              <s:Body>
               <t:RequestSecurityTokenResponse xmlns:t="http://schemas.xmlsoap.org/ws/2005/02/trust">
                <t:Lifetime>
                 <wsu:Created xmlns:wsu="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd">2020-07-24T17:41:50.636Z</wsu:Created>
                 <wsu:Expires xmlns:wsu="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd">2020-07-24T21:41:50.636Z</wsu:Expires>
             */

            var created = GetDateTimeOffset(soapDocument, namespaceManager, "/s:Envelope/s:Body/t:RequestSecurityTokenResponse/t:Lifetime/wsu:Created");
            var expires = GetDateTimeOffset(soapDocument, namespaceManager, "/s:Envelope/s:Body/t:RequestSecurityTokenResponse/t:Lifetime/wsu:Expires");

            return new RequestSecurityTokenResponseLifetime { Created = created, Expires = expires };
        }

        private static string GetNodeInnerText(XmlDocument soapDocument, XmlNamespaceManager namespaceManager, string path)
        {
            XmlNode node = soapDocument.SelectSingleNode(path, namespaceManager);
            return node?.InnerText;
        }

        private static string GetNodeInnerXml(XmlDocument soapDocument, XmlNamespaceManager namespaceManager, string path)
        {
            XmlNode node = soapDocument.SelectSingleNode(path, namespaceManager);
            return node?.InnerXml;
        }

        private static DateTimeOffset GetDateTimeOffset(XmlDocument soapDocument, XmlNamespaceManager namespaceManager, string path)
        {
            string innerText = GetNodeInnerText(soapDocument, namespaceManager, path);

            if (innerText == null)
            {
                return DateTimeOffset.MinValue; // not good
            }

            DateTimeOffset value = DateTimeOffset.Parse(
                innerText,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);

            return value;
        }

        /// <summary>
        /// Gets the XML namespace manager with all the namespaces added.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns></returns>
        private static XmlNamespaceManager GetXmlNamespaceManager(XmlDocument document)
        {
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(document.NameTable);

            // add all the SOAP namespaces that we may require, prefexes listed alphabetical

            namespaceManager.AddNamespace("a", "http://www.w3.org/2005/08/addressing");
            namespaceManager.AddNamespace("o", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
            namespaceManager.AddNamespace("s", "http://www.w3.org/2003/05/soap-envelope");
            namespaceManager.AddNamespace("t", "http://schemas.xmlsoap.org/ws/2005/02/trust");
            namespaceManager.AddNamespace("u", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
            namespaceManager.AddNamespace("wsp", "http://schemas.xmlsoap.org/ws/2004/09/policy");
            namespaceManager.AddNamespace("wsu", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");

            return namespaceManager;
        }
    }
}