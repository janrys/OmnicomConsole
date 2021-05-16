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

        public UserLoginCommand(CodebookUser codebookUser, ClaimsPrincipal user) : base(user, null)
        {
            this.CodebookUser = codebookUser;
        }

        public CodeGrantResponse CodeGrantResponse { get; }
        public CodebookUser CodebookUser { get; set; }
    }
}
