using MediatR;
using System.Security.Claims;

namespace AngularCrudApi.Application.Pipeline.Commands
{
    public class UserLogoutCommand : BaseAction<Unit>, IRequest<Unit>
    {
        public UserLogoutCommand(ClaimsPrincipal user) : base(user, null)
        {
        }

    }
}
