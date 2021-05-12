using AngularCrudApi.Application.DTOs;
using AngularCrudApi.Domain.Entities;
using MediatR;
using System.Security.Claims;

namespace AngularCrudApi.Application.Pipeline.Commands
{
    public class UserLoginRefreshCommand : BaseAction<CodebookUser>, IRequest<CodebookUser>
    {
        public UserLoginRefreshCommand(CodeGrantResponse codeGrantResponse, string token, ClaimsPrincipal user) : base(user, null)
        {
            this.CodeGrantResponse = codeGrantResponse;
            this.Token = token;
        }

        public CodeGrantResponse CodeGrantResponse { get; }
        public string Token { get; }
    }
}
