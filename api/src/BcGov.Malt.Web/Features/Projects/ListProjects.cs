using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BcGov.Malt.Web.Models;
using BcGov.Malt.Web.Services;
using MediatR;

namespace BcGov.Malt.Web.Features.Projects
{
    public static class ListProjects
    {
        public class Request : IRequest<List<Project>>
        {
        }

        public class Handler : IRequestHandler<Request, List<Project>>
        {
            private readonly IProjectService _projectService;

            public Handler(IProjectService projectService)
            {
                _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
            }

            public async Task<List<Project>> Handle(Request request, CancellationToken cancellationToken)
            {
                var projects = await _projectService.GetProjectsAsync();
                return projects;
            }
        }
    }
}
