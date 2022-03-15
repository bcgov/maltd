namespace BcGov.Jag.AccountManagement.Server.Services;

[Serializable]
public class OAuthApiException : Exception
{
    public OAuthApiException() { }
    public OAuthApiException(string message, int statusCode, string data, Dictionary<string, string> headers, string notUsed) : base(message) { }
    public OAuthApiException(string message, Exception inner) : base(message, inner) { }
    protected OAuthApiException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
