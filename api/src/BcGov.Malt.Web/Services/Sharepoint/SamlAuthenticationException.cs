using System;
using System.Runtime.Serialization;

namespace BcGov.Malt.Web.Services.Sharepoint
{
    [Serializable]
    public class SamlAuthenticationException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public SamlAuthenticationException()
        {
        }

        public SamlAuthenticationException(string message) : base(message)
        {
        }

        public SamlAuthenticationException(string message, Exception inner) : base(message, inner)
        {
        }

        protected SamlAuthenticationException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
