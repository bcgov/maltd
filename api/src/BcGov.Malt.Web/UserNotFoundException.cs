using System;
using System.Runtime.Serialization;

namespace BcGov.Malt.Web
{
    [Serializable]
    public class UserNotFoundException : Exception
    {
        public string Username { get; }

        public UserNotFoundException()
        {
        }

        public UserNotFoundException(string username) : base("User not found")
        {
            Username = username;
        }

        public UserNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }

        protected UserNotFoundException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
