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
    public class ReleaseAllQueryHandler : IRequestHandler<ReleaseAllQuery, IEnumerable<Release>>
            , IRequestHandler<ReleaseByIdQuery, Release>
    {
        private readonly ICodebookRepository codebookRepository;
        private readonly ILogger<ReleaseAllQueryHandler> log;

        public ReleaseAllQueryHandler(ICodebookRepository codebookRepository, ILogger<ReleaseAllQueryHandler> log)
        {
            this.codebookRepository = codebookRepository ?? throw new ArgumentNullException(nameof(codebookRepository));
            this.log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public Task<IEnumerable<Release>> Handle(ReleaseAllQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return this.codebookRepository.GetAllReleases();
            }
            catch (Exception exception)
            {
                this.log.LogError("Error loading requests", exception);
                throw;
            }
        }

        public Task<Release> Handle(ReleaseByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return this.codebookRepository.GetReleaseById(request.Id);
            }
            catch (Exception exception)
            {
                this.log.LogError($"Error loading request {request.Id}", exception);
                throw;
            }
        }
    }
}
