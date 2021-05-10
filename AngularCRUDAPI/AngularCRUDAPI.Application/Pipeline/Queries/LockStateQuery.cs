using AngularCrudApi.Domain.Entities;
using MediatR;
using System.Security.Claims;

namespace AngularCrudApi.Application.Pipeline.Queries
{
    public class LockStateQuery : BaseAction<LockState>, IRequest<LockState>
    {
        public LockStateQuery(ClaimsPrincipal user) : base(user, null)
        {
          
        }
    }
}
