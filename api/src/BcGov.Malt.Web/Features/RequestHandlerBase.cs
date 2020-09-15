using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.Cms;
using ILogger = Serilog.ILogger;

namespace BcGov.Malt.Web.Features
{
    public abstract class RequestHandlerBase<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private const string EnvironmentVariable = "API_TIMEOUT_SECONDS";

        protected ILogger<RequestHandlerBase<TRequest, TResponse>> Logger { get; }

        protected RequestHandlerBase(ILogger<RequestHandlerBase<TRequest, TResponse>> logger)
        {
            Logger = logger;
        }

        public abstract Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);

        protected CancellationTokenSource CreateCancellationTokenSource(CancellationToken cancellationToken)
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
            CancellationTokenSource timeout = new CancellationTokenSource(GetRequestTimeout());
#pragma warning restore CA2000 // Dispose objects before losing scope

            return CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);
        }

        private TimeSpan GetRequestTimeout()
        {
            var timeout = TimeSpan.FromSeconds(30);

            var stringValue = Environment.GetEnvironmentVariable(EnvironmentVariable);

            // log debug cause this will be logged for every request

            if (string.IsNullOrEmpty(stringValue) || !int.TryParse(stringValue, out var value))
            {
                Logger.LogDebug(
                    "Environment variable {EnvironmentVariable} is not set or is not an integer, defaulting to {Value}",
                    EnvironmentVariable, timeout);
            }
            else
            {
                timeout = TimeSpan.FromSeconds(Math.Abs(value));
                Logger.LogDebug("Environment variable {EnvironmentVariable} is set to {Value}", EnvironmentVariable, timeout);
            }

            return timeout;
        }
    }
}
