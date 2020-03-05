using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

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

    static class Authentication
    {
        /// <summary>
        /// Gets the XML RequestSecurityTokenResponse from the ADFS Secure Token Service (STS).
        /// </summary>
        /// <param name="spSiteUrl"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="stsUrl"></param>
        /// <returns></returns>
        public async static Task<string> GetStsSamlToken(string relyingParty, string username, string password, string stsUrl)
        {
            // Makes a request that conforms with the WS-Trust standard to 
            // the Security Token Service to get a SAML security token back 

            // generate the WS-Trust security token request SOAP message 
            string samlSoapRequest = CreateSamlSoapRequest(relyingParty, username, password, stsUrl);
            var content = new StringContent(samlSoapRequest, System.Text.Encoding.UTF8, "application/soap+xml");

            var client = new HttpClient();
            var responseMessage = await client.PostAsync(stsUrl, content);

            // A valid response needs to be a SOAP element, Content Type = application/soap+xml
            // Invalid parameters can still return 200 but will be an HTML document

            // The response could also be a SOAP Fault
            if (!responseMessage.IsSuccessStatusCode)
            {
                // TODO: change to better custom exception, read any possible content and log
                throw new Exception($"Request to {stsUrl} return HTTP Status {responseMessage.StatusCode}");
            }

            var responseContent = await responseMessage.Content.ReadAsStringAsync();

            if (TryParseXml(responseContent, out XmlDocument soapResponse))
            {
                // <a:Action s:mustUnderstand="1">http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/Issue</a:Action>
                // <a:Action s:mustUnderstand="1">http://www.w3.org/2005/08/addressing/soap/fault</a:Action>

                string action = SelectSoapAction(soapResponse);
                if (action == "http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/Issue")
                {
                    string securityTokenResponse = ExtractSoapBody(soapResponse);

                    return securityTokenResponse;
                }
                else if (action == "http://www.w3.org/2005/08/addressing/soap/fault")
                {
                    // extract /s:Envelope/s:Body/s:Fault/s:Code/s:Subcode/s:Value and /s:Envelope/s:Body/s:Fault/s:Reason/s:Text

                    // TODO: throw custom exception related to SOAP fault
                    throw new Exception("SOAP Fault : " + responseContent);
                }
                else
                {
                    // TODO: throw custom exception related to SOAP fault
                    throw new Exception("Unexpected SOAP Action : " + action);
                }
            }
            else
            {
                // response could be a HTML error page
                // TODO: throw custom exception related to ADFS error page
                throw new Exception("Invalid SOAP Reponse. Content is not valid XML");
            }
        }

        public static async Task GetSharepointFedAuthCookie(Uri samlServerUri, string samlToken, HttpClient client, CookieContainer cookieContainer)
        {
            // Single Sign On endpoint to post SAML token too
            var trustUri = new Uri(samlServerUri, "_trust/");

            // create the body of the POST
            var data = new Dictionary<string, string>
            {
                { "wa", "wsignin1.0" },
                { "wctx", new Uri(samlServerUri, "_layouts/Authenticate.aspx?Source=%2F").ToString() },
                { "wresult", samlToken }
            };

            using var content = new FormUrlEncodedContent(data);

            var _httpPostResponse = await client.PostAsync(trustUri, content);

            // get the schema and authority of the SAML server (ie Sharepoint)
            var wreplyUri = new Uri(samlServerUri.GetLeftPart(UriPartial.Authority));

            // if we are using an API gateway we need to copy the fedAuth
            // cookie to the wreply host.
            if (trustUri != wreplyUri)
            {
                CookieCollection cookies = cookieContainer.GetCookies(trustUri);
                string fedAuthCookieValue = cookies["FedAuth"].Value;

                cookieContainer.Add(wreplyUri, new Cookie("FedAuth", fedAuthCookieValue, "/"));
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
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(document.NameTable);

            namespaceManager.AddNamespace("a", "http://www.w3.org/2005/08/addressing");
            namespaceManager.AddNamespace("s", "http://www.w3.org/2003/05/soap-envelope");

            XmlNode actionNode = document.SelectSingleNode("/s:Envelope/s:Header/a:Action", namespaceManager);
            string action = actionNode?.InnerText ?? string.Empty;

            if (string.IsNullOrEmpty(action))
            {
                // TODO: log that the XML did not contain an Action, it is possible the namespace prefixes can change?
            }

            return action;
        }

        /// <summary>
        /// Create a SOAP Envelope message for requesting a SAML token
        /// </summary>
        /// <param name="url"></param>
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
            // this is safer than using string replacemnts as the password could contain
            // character sequences that are invalid in an XML element
            XmlDocument soapDocument = new XmlDocument();
            soapDocument.LoadXml(samlRTString);

            // create a name space manager for the prefixes we need to reference
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(soapDocument.NameTable);

            namespaceManager.AddNamespace("a", "http://www.w3.org/2005/08/addressing");
            namespaceManager.AddNamespace("o", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
            namespaceManager.AddNamespace("s", "http://www.w3.org/2003/05/soap-envelope");
            namespaceManager.AddNamespace("t", "http://schemas.xmlsoap.org/ws/2005/02/trust");
            namespaceManager.AddNamespace("wsp", "http://schemas.xmlsoap.org/ws/2004/09/policy");

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
            // create a name space manager for the prefixes we need to reference
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(soapDocument.NameTable);
            namespaceManager.AddNamespace("s", "http://www.w3.org/2003/05/soap-envelope");

            // get the <s:Body> element and return the inner XML that contains
            // a <t:RequestSecurityTokenResponse> element
            XmlNode bodyNode = soapDocument.SelectSingleNode("/s:Envelope/s:Body", namespaceManager);

            return bodyNode.InnerXml;
        }

    }
}