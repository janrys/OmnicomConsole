using AngularCrudApi.Domain.Entities;
using MediatR;
using System.Security.Claims;

namespace AngularCrudApi.Application.Pipeline.Commands
{
    public class ReleaseLockCommand : BaseAction<LockState>, IRequest<LockState>
    {
        public ReleaseLockCommand(ClaimsPrincipal user) : base(user, null)
        {

        }
    }
}
