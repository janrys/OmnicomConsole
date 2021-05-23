using MediatR;
using System.Security.Claims;

namespace AngularCrudApi.Application.Pipeline.Commands
{
    public class RequestDeleteCommand : BaseAction<Unit>, IRequest<Unit>
    {
        public RequestDeleteCommand(int id, ClaimsPrincipal user) : base(user, null)
        {
            this.Id = id;
        }

        public int Id { get; }
    }
}
