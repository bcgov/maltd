namespace BcGov.Jag.AccountManagement.Server.Services;

[Serializable]
public class UserSearchInvalidException : UserSearchException
{
    public UserSearchInvalidException() { }
    public UserSearchInvalidException(string message) : base(message) { }
    public UserSearchInvalidException(string message, Exception inner) : base(message, inner) { }
    protected UserSearchInvalidException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
