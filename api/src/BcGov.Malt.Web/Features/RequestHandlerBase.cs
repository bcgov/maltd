using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace BcGov.Malt.Web.Features
{
    public abstract class RequestHandlerBase<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public abstract Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);

        protected CancellationTokenSource CreateCancellationTokenSource(CancellationToken cancellationToken)
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
            CancellationTokenSource timeout = new CancellationTokenSource(TimeSpan.FromSeconds(29));
#pragma warning restore CA2000 // Dispose objects before losing scope

            return CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);
        }
    }
}
