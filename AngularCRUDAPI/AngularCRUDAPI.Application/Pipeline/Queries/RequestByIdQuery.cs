using AngularCrudApi.Domain.Entities;
using MediatR;
using System.Security.Claims;

namespace AngularCrudApi.Application.Pipeline.Queries
{
    public class RequestByIdQuery : BaseAction<Request>, IRequest<Request>
    {
        public RequestByIdQuery(int id, ClaimsPrincipal user) : base(user, null)
        {
            this.Id = id;
        }

        public int Id { get; }
    }
}
