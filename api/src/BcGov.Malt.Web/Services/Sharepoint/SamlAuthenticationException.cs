using System;
using System.Runtime.Serialization;

namespace BcGov.Malt.Web.Services.Sharepoint
{
    [Serializable]
    public class SamlAuthenticationException : Exception
    {
        public string Code { get; }
        public string Subcode { get; }
        public string Reason { get; }

        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public SamlAuthenticationException(string message) : this(message, string.Empty, string.Empty, string.Empty)
        {
        }

        public SamlAuthenticationException(string message, string code, string subcode, string reason) : base(message)
        {
            Code = code;
            Subcode = subcode;
            Reason = reason;
        }

        protected SamlAuthenticationException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
