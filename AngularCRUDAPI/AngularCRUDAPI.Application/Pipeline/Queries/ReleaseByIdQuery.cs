using AngularCrudApi.Domain.Entities;
using MediatR;
using System.Security.Claims;

namespace AngularCrudApi.Application.Pipeline.Queries
{
    public class ReleaseByIdQuery : BaseAction<Release>, IRequest<Release>
    {
        public ReleaseByIdQuery(int id, ClaimsPrincipal user) : base(user, null)
        {
            this.Id = id;
        }

        public int Id { get; }
    }
}
