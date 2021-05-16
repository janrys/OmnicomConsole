using AngularCrudApi.Application.Pipeline.Commands;
using AngularCrudApi.Application.Pipeline.Queries;
using AngularCrudApi.Domain.Entities;
using AngularCrudApi.WebApi.Extensions;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AngularCrudApi.WebApi.Helpers
{
    public static class AuthenticationMessageHandler
    {
        public static async Task MessageReceived(Microsoft.AspNetCore.Authentication.JwtBearer.MessageReceivedContext context, IWebHostEnvironment environment)
        {
            if (environment.IsCodebookDevelopment())
            {
                if (context.Request.Headers.Any(h => h.Key.Equals("authorization", StringComparison.InvariantCultureIgnoreCase)))
                {
                    string token = context.Request.Headers.First(h => h.Key.Equals("authorization", StringComparison.InvariantCultureIgnoreCase))
                        .Value.First().Split(' ').Last();

                    if (token.Equals("111111", StringComparison.InvariantCultureIgnoreCase))
                    {
                        context.Principal = new ClaimsPrincipal();
                        CodebookUser codebookUser = new CodebookUser().FillFakeAdmin();
                        codebookUser.Identifier = token;
                        context.Principal.AddIdentity(codebookUser.ToClaimIdentity());
                        await context.HttpContext.RequestServices.GetRequiredService<IMediator>()
                            .Send(new UserLoginCommand(codebookUser, context.Principal));
                        context.Success();
                    }
                    else if (token.Equals("222222", StringComparison.InvariantCultureIgnoreCase))
                    {
                        context.Principal = new ClaimsPrincipal();
                        CodebookUser codebookUser = new CodebookUser().FillFakeReader();
                        codebookUser.Identifier = token;
                        context.Principal.AddIdentity(codebookUser.ToClaimIdentity());
                        await context.HttpContext.RequestServices.GetRequiredService<IMediator>().Send(new UserLoginCommand(codebookUser, context.Principal));
                        context.Success();
                    }
                    else if (token.Equals("333333", StringComparison.InvariantCultureIgnoreCase))
                    {
                        context.Principal = new ClaimsPrincipal();
                        CodebookUser codebookUser = new CodebookUser().FillFakeEditor();
                        codebookUser.Identifier = token;
                        context.Principal.AddIdentity(codebookUser.ToClaimIdentity());
                        await context.HttpContext.RequestServices.GetRequiredService<IMediator>().Send(new UserLoginCommand(codebookUser, context.Principal));
                        context.Success();
                    }
                }
            }
        }


        public static async Task TokenValidated(Microsoft.AspNetCore.Authentication.JwtBearer.TokenValidatedContext context)
        {
            IMediator mediator = context.HttpContext.RequestServices.GetRequiredService<IMediator>();
            CodebookUser codebookUser = await mediator.Send(new UserByTokenQuery(((JwtSecurityToken)context.SecurityToken).RawData, null));

            if (codebookUser == null)
            {
                context.Fail("User was not found by access token. Try to logout and login again.");
            }
            else
            {
                context.Principal.AddIdentity(codebookUser.ToClaimIdentity());
            }
        }

        public static Task AuthenticationFailed(Microsoft.AspNetCore.Authentication.JwtBearer.AuthenticationFailedContext context, ILogger logger)
        {
            logger.LogWarning("Authentication failed, reason {message}, ip {ipAddress}, correlationId {correlationId}"
                , context.Exception?.Message ?? "token validation failed", context.Request.GetClientIpAddress(), context.Request.GetCorrelationId());
            return Task.CompletedTask;
        }

    }
}
