using AngularCrudApi.Application.DTOs;
using AngularCrudApi.Domain.Entities;
using MediatR;
using System.Security.Claims;

namespace AngularCrudApi.Application.Pipeline.Commands
{
    public class UserLoginCommand : BaseAction<CodebookUser>, IRequest<CodebookUser>
    {
        public UserLoginCommand(CodeGrantResponse codeGrantResponse, ClaimsPrincipal user) : base(user, null)
        {
            this.CodeGrantResponse = codeGrantResponse;
        }

        public CodeGrantResponse CodeGrantResponse { get; }
    }
}
