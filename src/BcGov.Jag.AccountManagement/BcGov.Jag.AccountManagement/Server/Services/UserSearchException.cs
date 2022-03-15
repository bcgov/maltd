namespace BcGov.Jag.AccountManagement.Server.Services;

[Serializable]
public abstract class UserSearchException : Exception
{
    public UserSearchException() { }
    public UserSearchException(string message) : base(message) { }
    public UserSearchException(string message, Exception inner) : base(message, inner) { }
    protected UserSearchException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
