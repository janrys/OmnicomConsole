using AngularCrudApi.Application.Exceptions;
using AngularCrudApi.Application.Interfaces.Repositories;
using AngularCrudApi.Application.Pipeline.Commands;
using AngularCrudApi.Application.Pipeline.Queries;
using AngularCrudApi.Domain.Entities;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AngularCrudApi.Application.Pipeline.Handlers
{
    public class LockStateHandler : IRequestHandler<LockStateQuery, LockState>
        , IRequestHandler<CreateLockCommand, LockState>
        , IRequestHandler<ReleaseLockCommand, LockState>
    {
        private readonly ICodebookRepository codebookRepository;
        private readonly ILogger<LockStateHandler> log;

        public LockStateHandler(ICodebookRepository codebookRepository, ILogger<LockStateHandler> log)
        {
            this.codebookRepository = codebookRepository ?? throw new ArgumentNullException(nameof(codebookRepository));
            this.log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public Task<LockState> Handle(LockStateQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return this.codebookRepository.GetLock();
            }
            catch (Exception exception)
            {
                this.log.LogError($"Error loading lock state", exception);
                throw;
            }
        }

        public async Task<LockState> Handle(CreateLockCommand request, CancellationToken cancellationToken)
        {
            LockState lockState = null;
            try
            {
                lockState = await this.codebookRepository.GetLock();
            }
            catch (Exception exception)
            {
                this.log.LogError($"Error loading lock state", exception);
                throw;
            }

            if (lockState.IsLocked && !lockState.ForUserId.Equals(request.User.GetIdentifier()))
            {
                throw new ValidationException(new string[] { $"Lock is already being held by {lockState.ForUserName}" });
            }

            try
            {
                Request storedRequest = await this.codebookRepository.GetRequestById(request.RequestId);

                if(storedRequest == null)
                {
                    throw new ValidationException($"Request with id {request.RequestId} not found");
                }

                return await this.codebookRepository.CreateLock(request.User.GetIdentifier(), request.User.GetName(), storedRequest.Id);
            }
            catch (Exception exception)
            {
                this.log.LogError($"Error creating lock state", exception);
                throw;
            }
        }

        public async Task<LockState> Handle(ReleaseLockCommand request, CancellationToken cancellationToken)
        {
            LockState lockState = null;
            try
            {
                lockState = await this.codebookRepository.GetLock();
            }
            catch (Exception exception)
            {
                this.log.LogError($"Error loading lock state", exception);
                throw;
            }

            if (lockState.IsLocked && !lockState.ForUserId.Equals(request.User.GetIdentifier()))
            {
                throw new ValidationException(new string[] { $"Lock is already being held by {lockState.ForUserName}" });
            }

            try
            {
                return await this.codebookRepository.ReleaseLock();
            }
            catch (Exception exception)
            {
                this.log.LogError($"Error releasing lock state", exception);
                throw;
            }
        }
    }
}
