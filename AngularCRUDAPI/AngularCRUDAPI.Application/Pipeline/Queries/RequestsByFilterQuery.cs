using AngularCrudApi.Domain.Entities;
using AngularCrudApi.Domain.Enums;
using MediatR;
using System.Collections.Generic;
using System.Security.Claims;

namespace AngularCrudApi.Application.Pipeline.Queries
{
    public class RequestsByFilterQuery : BaseAction<IEnumerable<Request>>, IRequest<IEnumerable<Request>>
    {
        public RequestsByFilterQuery(int[] releaseIds, RequestStateEnum requestState, ClaimsPrincipal user) : base(user, null)
        {
            this.ReleaseIds = releaseIds;
            this.RequestState = requestState;
        }

        public int[] ReleaseIds { get; }
        public RequestStateEnum RequestState { get; }
    }
}
