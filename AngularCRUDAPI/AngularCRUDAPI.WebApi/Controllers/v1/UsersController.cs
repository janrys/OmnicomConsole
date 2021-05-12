using AngularCrudApi.Application.DTOs;
using AngularCrudApi.Application.Exceptions;
using AngularCrudApi.Application.Interfaces;
using AngularCrudApi.Domain.Entities;
using AngularCrudApi.WebApi.Extensions;
using AngularCrudApi.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AngularCrudApi.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    public class UsersController : BaseApiController
    {
        private readonly ILogger<UsersController> log;
        private readonly IAuthorizationServerClient authorizationServerClient;

        public UsersController(IMediator mediator, ILogger<UsersController> log, IAuthorizationServerClient authorizationServerClient) : base(mediator)
        {
            this.log = log ?? throw new ArgumentNullException(nameof(log));
            this.authorizationServerClient = authorizationServerClient ?? throw new ArgumentNullException(nameof(authorizationServerClient));
        }


        /// <summary>
        /// Login user
        /// </summary>
        /// <param name="state">Optional state to receive back after redirect</param>
        /// <returns>Redirect to authorization server</returns>
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [HttpGet("logIn")]
        public IActionResult LogIn(string state = null)
        {
            string authorizationUrl = this.authorizationServerClient.GetAuthorizationLink(state);
            return this.Redirect(authorizationUrl);
        }

        /// <summary>
        /// Logout current user
        /// </summary>
        /// <returns>Logout result</returns>
        [HttpGet("logOut")]
        public async Task<IActionResult> LogOut()
        {
            await this.Command().User.Logout();
            return this.Ok();
        }

        /// <summary>
        /// Get access token and login user
        /// </summary>
        /// <param name="code">Authorization code</param>
        /// <returns>Access token with expiration time</returns>
        [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [AllowAnonymous]
        [HttpGet("token")]
        public async Task<IActionResult> Token(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return this.BadRequest($"Parameter {nameof(code)} is mandatory");
            }

            try
            {
                CodeGrantResponse codeGrantResponse = await this.authorizationServerClient.GetToken(code);
                CodebookUser codebookUser = await this.CommandAnonymous().User.Login(codeGrantResponse);
                return this.Ok(new TokenResponse() { Token = codeGrantResponse.IdToken, Expiration = DateTime.Now.AddSeconds(codeGrantResponse.ExpiresIn) });
            }
            catch (Exception exception)
            {
                this.log.LogWarning(exception, "Authentication failed");
                return this.Forbid("Authentication failed");
            }
        }

        /// <summary>
        /// Refresh access token
        /// </summary>
        /// <param name="authorization">Authorization header in form "Bearer {token}"</param>
        /// <returns>New access token with expiration time</returns>
        [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [AllowAnonymous]
        [HttpGet("refreshToken")]
        public async Task<IActionResult> RefreshToken([FromHeader] string authorization)
        {
            if (string.IsNullOrEmpty(authorization))
            {
                return this.BadRequest($"Parameter {nameof(authorization)} is mandatory");
            }

            string token = ExtractTokenFromHeader(authorization);
            CodebookUser codedbookUser = await this.QueryAnonymous().User.ByToken(token);

            if (codedbookUser == null)
            {
                string message = "User not found" + ". Token " + token;
                this.log.LogWarning(message);
                return this.Unauthorized(message);
            }

            try
            {
                CodeGrantResponse codeGrantResponse = await this.authorizationServerClient.GetRefreshedToken(token, codedbookUser.RefreshToken);
                codedbookUser = await this.CommandAnonymous().User.Login(codeGrantResponse, token);
                return this.Ok(new TokenResponse() { Token = codeGrantResponse.IdToken, Expiration = DateTime.Now.AddSeconds(codeGrantResponse.ExpiresIn) });
            }
            catch (Exception exception)
            {
                string message = "Authentication failed";
                this.log.LogWarning(exception, message);
                return this.Unauthorized(message);
            }
        }

        /// <summary>
        /// Get information about current user
        /// <param name="authorization">Authorization header in form "Bearer {token}"</param>
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(PublicCodebookUser), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpGet("me")]
        public async Task<IActionResult> Me([FromHeader] string authorization)
        {
            if (string.IsNullOrEmpty(authorization))
            {
                return this.BadRequest($"Parameter {nameof(authorization)} is mandatory");
            }

            string token = ExtractTokenFromHeader(authorization);
            CodebookUser codedbookUser = await this.Query().User.ByToken(token);

            if (codedbookUser == null)
            {
                string message = "User not found" + ". Token " + token;
                this.log.LogWarning(message);
                return this.Unauthorized(message);
            }

            return this.Ok(codedbookUser.AsPublic());
        }

        /// <summary>
        /// Extract token from authorization header in form of "Bearer ejfasdfasdf..."
        /// </summary>
        /// <param name="authorization"></param>
        /// <returns></returns>
        private static string ExtractTokenFromHeader(string authorization)
        {
            return authorization.Split(' ').LastOrDefault();
        }

    }
}
