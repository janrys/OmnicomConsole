using AngularCrudApi.Application.Helpers;
using AngularCrudApi.Application.Interfaces.Repositories;
using AngularCrudApi.Application.Pipeline.Commands;
using AngularCrudApi.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AngularCrudApi.Application.Pipeline.Handlers
{
    public class ReleaseCreateCommandHandler : IRequestHandler<ReleaseCreateCommand, Release>
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
                return this.codebookRepository.CreateRelease(request.Release);
            }
            catch (Exception exception)
            {
                this.log.LogError($"Error creating release {request.Release.ToLogString()}", exception);
                throw;
            }
        }
        
    }
}
