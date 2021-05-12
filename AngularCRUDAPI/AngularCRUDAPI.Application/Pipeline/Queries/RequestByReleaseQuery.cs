using AngularCrudApi.Domain.Entities;
using MediatR;
using System.Collections.Generic;
using System.Security.Claims;

namespace AngularCrudApi.Application.Pipeline.Queries
{
    public class RequestByReleaseQuery : BaseAction<IEnumerable<Request>>, IRequest<IEnumerable<Request>>
    {
        public RequestByReleaseQuery(int releaseId, ClaimsPrincipal user) : base(user, null)
        {
            this.ReleaseId = releaseId;
        }

        public int ReleaseId { get; }
    }
}
