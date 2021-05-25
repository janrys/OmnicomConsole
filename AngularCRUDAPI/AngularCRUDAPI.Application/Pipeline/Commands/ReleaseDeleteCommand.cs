using MediatR;
using System.Security.Claims;

namespace AngularCrudApi.Application.Pipeline.Commands
{
    public class ReleaseDeleteCommand : BaseAction<Unit>, IRequest<Unit>
    {
        public ReleaseDeleteCommand(int id, ClaimsPrincipal user) : base(user, null)
        {
            this.Id = id;
        }

        public int Id { get; }
    }
}
