using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BcGov.Malt.Web.Models;
using BcGov.Malt.Web.Models.Configuration;
using MediatR;

namespace BcGov.Malt.Web.Features.Projects
{
    public static class ListProjects
    {
#pragma warning disable CA1034 // do not next type
        public class Request : IRequest<List<Project>>
        {
        }

        public class Handler : IRequestHandler<Request, List<Project>>
        {
            private readonly ProjectConfigurationCollection _projects;

            public Handler(ProjectConfigurationCollection projects)
            {
                _projects = projects ?? throw new ArgumentNullException(nameof(projects));
            }

            public Task<List<Project>> Handle(Request request, CancellationToken cancellationToken)
            {
                var projects = _projects.Select(_ => new Project(_.Id, _.Name)).ToList();
                return Task.FromResult(projects);
            }
        }
    }
}
