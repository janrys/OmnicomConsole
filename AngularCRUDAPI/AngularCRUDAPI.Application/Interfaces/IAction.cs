using AngularCrudApi.Application.Security;
using MediatR;
using System.Collections.Generic;
using System.Security.Claims;

namespace AngularCrudApi.Application.Interfaces
{
    public interface IAction<TResult>
    {
        IRequest<TResult> AsRequest();
        IEnumerable<RoleEnum> AllowedRoles { get; }
        ClaimsPrincipal User { get; }
    }


}
