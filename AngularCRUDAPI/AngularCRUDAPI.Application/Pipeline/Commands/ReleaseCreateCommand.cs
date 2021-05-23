using AngularCrudApi.Domain.Entities;
using MediatR;
using System.Security.Claims;

namespace AngularCrudApi.Application.Pipeline.Commands
{
    public class ReleaseCreateCommand : BaseAction<Release>, IRequest<Release>
    {
        public ReleaseCreateCommand(Release release, ClaimsPrincipal user) : base(user, null)
        {
            this.Release = release;
        }

        public Release Release { get; }
    }
}
