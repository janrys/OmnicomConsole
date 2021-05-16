using AngularCrudApi.Application.Interfaces;
using AngularCrudApi.Application.Interfaces.Repositories;
using AngularCrudApi.Application.Pipeline.Commands;
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
    public class UserLoginCommandHandler : IRequestHandler<UserLoginCommand, CodebookUser>
        , IRequestHandler<UserLoginRefreshCommand, CodebookUser>
        , IRequestHandler<UserLogoutCommand, Unit>
    {
        private readonly IUserDataProvider userProvider;
        private readonly ILogger<UserLoginCommandHandler> log;

        public UserLoginCommandHandler(IUserDataProvider userProvider, ILogger<UserLoginCommandHandler> log)
        {
            this.userProvider = userProvider ?? throw new ArgumentNullException(nameof(userProvider));
            this.log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public Task<CodebookUser> Handle(UserLoginCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.CodeGrantResponse != null)
                {
                    return this.userProvider.Login(request.CodeGrantResponse);
                }
                else if(request.CodebookUser != null)
                {
                    return this.userProvider.Login(request.CodebookUser);
                }
                else
                {
                    throw new ArgumentException($"Both parameters {nameof(request.CodebookUser)} and {nameof(request.CodeGrantResponse)} cannot be null");
                }                
            }
            catch (Exception exception)
            {
                this.log.LogError("Error login user", exception);
                throw;
            }
        }

        public Task<CodebookUser> Handle(UserLoginRefreshCommand request, CancellationToken cancellationToken)
        {
            try
            {
                return this.userProvider.LoginRefresh(request.CodeGrantResponse, request.Token);
            }
            catch (Exception exception)
            {
                this.log.LogError("Error refresh login user", exception);
                throw;
            }
        }

        public async Task<Unit> Handle(UserLogoutCommand request, CancellationToken cancellationToken)
        {
            try
            {
                string identifier = request.User.GetIdentifier();
                await this.userProvider.Logout(identifier);
                return Unit.Value;
            }
            catch (Exception exception)
            {
                this.log.LogError("Error logout user", exception);
                throw;
            }
        }
    }

    public class UserByTokenQueryHandler : IRequestHandler<UserByTokenQuery, CodebookUser>
    {
        private readonly IUserDataProvider userProvider;
        private readonly ILogger<UserLoginCommandHandler> log;

        public UserByTokenQueryHandler(IUserDataProvider userProvider, ILogger<UserLoginCommandHandler> log)
        {
            this.userProvider = userProvider ?? throw new ArgumentNullException(nameof(userProvider));
            this.log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public Task<CodebookUser> Handle(UserByTokenQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return this.userProvider.GetByToken(request.Token);
            }
            catch (Exception exception)
            {
                this.log.LogError("Error loading user", exception);
                throw;
            }
        }
    }
}
