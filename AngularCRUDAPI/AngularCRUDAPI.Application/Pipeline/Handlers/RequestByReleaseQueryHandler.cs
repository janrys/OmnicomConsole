using AngularCrudApi.Application.Interfaces.Repositories;
using AngularCrudApi.Application.Pipeline.Queries;
using AngularCrudApi.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AngularCrudApi.Application.Pipeline.Handlers
{
    public class RequestByReleaseQueryHandler : IRequestHandler<RequestByReleaseQuery, IEnumerable<Request>>
        , IRequestHandler<RequestByIdQuery, Request>
    {
        private readonly ICodebookRepository codebookRepository;
        private readonly ILogger<RequestByReleaseQueryHandler> log;

        public RequestByReleaseQueryHandler(ICodebookRepository codebookRepository, ILogger<RequestByReleaseQueryHandler> log)
        {
            this.codebookRepository = codebookRepository ?? throw new ArgumentNullException(nameof(codebookRepository));
            this.log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public Task<IEnumerable<Request>> Handle(RequestByReleaseQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return this.codebookRepository.GetRequests(request.ReleaseId);
            }
            catch (Exception exception)
            {
                this.log.LogError("Error loading requests", exception);
                throw;
            }
        }

        public Task<Request> Handle(RequestByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return this.codebookRepository.GetRequestById(request.Id);
            }
            catch (Exception exception)
            {
                this.log.LogError($"Error loading request by id {request.Id}", exception);
                throw;
            }
        }
    }
}
