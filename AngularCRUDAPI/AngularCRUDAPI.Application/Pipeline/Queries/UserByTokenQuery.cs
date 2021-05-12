using AngularCrudApi.Domain.Entities;
using MediatR;
using System.Security.Claims;

namespace AngularCrudApi.Application.Pipeline.Queries
{
    public class UserByTokenQuery : BaseAction<CodebookUser>, IRequest<CodebookUser>
    {
        public UserByTokenQuery(string token, ClaimsPrincipal user) : base(user, null)
        {
            this.Token = token;
        }

        public string Token { get; }
    }
}
