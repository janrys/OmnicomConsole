using AngularCrudApi.Domain.Entities;
using MediatR;
using System.Security.Claims;

namespace AngularCrudApi.Application.Pipeline.Commands
{
    public class RequestUpdateCommand : BaseAction<Request>, IRequest<Request>
    {
        public RequestUpdateCommand(Request request, ClaimsPrincipal user) : base(user, null)
        {
            this.Request = request;
        }

        public Request Request { get; }
    }
}
