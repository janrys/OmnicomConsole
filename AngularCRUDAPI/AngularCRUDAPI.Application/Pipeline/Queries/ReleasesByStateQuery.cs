using AngularCrudApi.Domain.Entities;
using AngularCrudApi.Domain.Enums;
using MediatR;
using System.Collections.Generic;
using System.Security.Claims;

namespace AngularCrudApi.Application.Pipeline.Queries
{
    public class ReleasesByStateQuery : BaseAction<IEnumerable<Release>>, IRequest<IEnumerable<Release>>
    {
        public ReleasesByStateQuery(ReleaseStateEnum releaseState, ClaimsPrincipal user) : base(user, null)
        {
            this.ReleaseState = releaseState;
        }

        public ReleaseStateEnum ReleaseState { get; }
    }
}
