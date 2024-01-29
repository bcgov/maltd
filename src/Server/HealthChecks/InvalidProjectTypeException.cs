using System.Runtime.Serialization;
using BcGov.Jag.AccountManagement.Server.Models.Configuration;

namespace BcGov.Jag.AccountManagement.Server.HealthChecks;

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
