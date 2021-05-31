using AngularCrudApi.Application.Security;
using MediatR;
using System.Collections.Generic;
using System.Security.Claims;

namespace AngularCrudApi.Application.Interfaces
{

    public interface IAction
    {
        IEnumerable<RoleEnum> AllowedRoles { get; }
        ClaimsPrincipal User { get; }
    }

    public interface IAction<TResult> : IAction
    {
        IRequest<TResult> AsRequest();
    }


}
