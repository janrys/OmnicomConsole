using AngularCrudApi.Application.DTOs;
using MediatR;
using System.Security.Claims;

namespace AngularCrudApi.Application.Pipeline.Queries
{
    public class LastPackageQuery : BaseAction<PackageInfo>, IRequest<PackageInfo>
    {
        public LastPackageQuery(ClaimsPrincipal user) : base(user, null)
        {
        }

    }

    
}
