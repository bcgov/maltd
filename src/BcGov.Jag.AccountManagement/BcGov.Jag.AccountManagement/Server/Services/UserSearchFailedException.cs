namespace BcGov.Jag.AccountManagement.Server.Services;

[Serializable]
public class UserSearchFailedException : Exception
{
    public UserSearchFailedException() { }
    public UserSearchFailedException(string message) : base(message) { }
    public UserSearchFailedException(string message, Exception inner) : base(message, inner) { }
    protected UserSearchFailedException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
