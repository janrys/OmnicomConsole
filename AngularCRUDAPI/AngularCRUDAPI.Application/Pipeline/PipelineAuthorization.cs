using AngularCrudApi.Application.Exceptions;
using AngularCrudApi.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AngularCrudApi.Application.Pipeline
{
    public class PipelineAuthorization<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IActionAuthorizer authorizer;

        public PipelineAuthorization(ILoggerFactory loggerFactory, IActionAuthorizer authorizer)
        {
            this._loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            this.authorizer = authorizer ?? throw new ArgumentNullException(nameof(authorizer));
        }

        public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            if (request is IAction authenticatedRequest)
            {
                try
                {
                    this.authorizer.Authorize(authenticatedRequest);
                }
                catch (Exception exception)
                {
                    string message = $"Action {request.GetType().Name} authorization failed.";
                    this._loggerFactory.CreateLogger<TRequest>().LogWarning(exception, message);
                    throw new ForbiddenException(message, exception);
                }
            }

            return next();
        }
    }
}
