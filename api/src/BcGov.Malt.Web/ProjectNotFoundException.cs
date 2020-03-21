using System;
using System.Runtime.Serialization;

namespace BcGov.Malt.Web
{
    [Serializable]
    public class ProjectNotFoundException : Exception
    {
        public string ProjectId { get; }

        public ProjectNotFoundException()
        {
        }

        public ProjectNotFoundException(string projectId) : base("Project not found")
        {
            ProjectId = projectId;
        }

        public ProjectNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }

        protected ProjectNotFoundException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
