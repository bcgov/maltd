using System;
using System.Runtime.Serialization;
using BcGov.Malt.Web.Models.Configuration;

namespace BcGov.Malt.Web.HealthChecks
{
    [Serializable]
    public class InvalidProjectTypeException : Exception
    {
        public ProjectType ProjectType { get; }

        public InvalidProjectTypeException(ProjectType projectType)
        {
            ProjectType = projectType;
        }

        protected InvalidProjectTypeException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}