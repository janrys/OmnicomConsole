using AngularCrudApi.Application.Helpers;
using AngularCrudApi.Application.Interfaces.Repositories;
using AngularCrudApi.Application.Pipeline.Commands;
using AngularCrudApi.Domain.Entities;
using AngularCrudApi.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AngularCrudApi.Application.Pipeline.Handlers
{
    public class ReleaseCreateCommandHandler : IRequestHandler<ReleaseCreateCommand, Release>
        , IRequestHandler<ReleaseUpdateCommand, Release>
        , IRequestHandler<ReleaseDeleteCommand, Unit>
    {
        private readonly ICodebookRepository codebookRepository;
        private readonly ILogger<ReleaseCreateCommandHandler> log;

        public ReleaseCreateCommandHandler(ICodebookRepository codebookRepository, ILogger<ReleaseCreateCommandHandler> log)
        {
            this.codebookRepository = codebookRepository ?? throw new ArgumentNullException(nameof(codebookRepository));
            this.log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public Task<Release> Handle(ReleaseCreateCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (String.IsNullOrEmpty(request.Release.Status) || !ReleaseStateEnum.GetAll().Any(s => s.Name.Equals(request.Release.Status, StringComparison.InvariantCulture)))
                {
                    request.Release.Status = ReleaseStateEnum.New.Name;
                }

                return this.codebookRepository.CreateRelease(request.Release);
            }
            catch (Exception exception)
            {
                this.log.LogError($"Error creating release {request.Release.ToLogString()}", exception);
                throw;
            }
        }

        public Task<Release> Handle(ReleaseUpdateCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (String.IsNullOrEmpty(request.Release.Status) || !ReleaseStateEnum.GetAll().Any(s => s.Name.Equals(request.Release.Status, StringComparison.InvariantCulture)))
                {
                    request.Release.Status = ReleaseStateEnum.New.Name;
                }

                return this.codebookRepository.UpdateRelease(request.Release);
            }
            catch (Exception exception)
            {
                this.log.LogError($"Error updating release {request.Release.ToLogString()}", exception);
                throw;
            }
        }

        public async Task<Unit> Handle(ReleaseDeleteCommand request, CancellationToken cancellationToken)
        {
            try
            {
                IEnumerable<Request> requests = await this.codebookRepository.GetRequests(request.Id);

                if (requests.Any())
                {
                    await this.codebookRepository.DeleteRequestsByReleaseId(request.Id);
                }

                await this.codebookRepository.DeleteRelease(request.Id);
                return Unit.Value;
            }
            catch (Exception exception)
            {
                this.log.LogError($"Error deleting release {request.Id}", exception);
                throw;
            }
        }
    }
}
