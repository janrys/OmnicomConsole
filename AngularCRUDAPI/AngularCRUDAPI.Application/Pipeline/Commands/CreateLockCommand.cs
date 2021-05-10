using AngularCrudApi.Domain.Entities;
using MediatR;
using System.Security.Claims;

namespace AngularCrudApi.Application.Pipeline.Commands
{
    public class CreateLockCommand : BaseAction<LockState>, IRequest<LockState>
    {
        public CreateLockCommand(ClaimsPrincipal user) : base(user, null)
        {

        }
    }
}
