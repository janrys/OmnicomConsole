using AngularCrudApi.Domain.Entities;
using MediatR;
using System.Collections.Generic;
using System.Security.Claims;

namespace AngularCrudApi.Application.Pipeline.Queries
{
    public class ReleaseAllQuery : BaseAction<IEnumerable<Release>>, IRequest<IEnumerable<Release>>
    {
        public ReleaseAllQuery(ClaimsPrincipal user) : base(user, null)
        {
            
        }
    }
}
