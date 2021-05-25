using AngularCrudApi.Domain.Entities;
using MediatR;
using System.Security.Claims;

namespace AngularCrudApi.Application.Pipeline.Commands
{
    public class ReleaseUpdateCommand : BaseAction<Release>, IRequest<Release>
    {
        public ReleaseUpdateCommand(Release release, ClaimsPrincipal user) : base(user, null)
        {
            this.Release = release;
        }

        public Release Release { get; }
    }
}
