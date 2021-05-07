using AngularCrudApi.Application.Interfaces;
using AngularCrudApi.Domain.Security;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AngularCrudApi.Application.Pipeline
{
    public abstract class BaseAction<TResult> : IAction<TResult>, IRequest<TResult>
    {
        public BaseAction() : this(null, null)
        {

        }

        public BaseAction(ClaimsPrincipal user, IEnumerable<RoleEnum> allowedRoles)
        {
            this.User = user;
            this.AllowedRoles = allowedRoles ?? new RoleEnum[] { };
        }

        public ClaimsPrincipal User { get; }

        public IEnumerable<RoleEnum> AllowedRoles { get; }

        public IRequest<TResult> AsRequest() => this;
    }
}
