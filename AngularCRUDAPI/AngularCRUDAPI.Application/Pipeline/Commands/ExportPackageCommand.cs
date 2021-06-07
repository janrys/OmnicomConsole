using AngularCrudApi.Application.DTOs;
using MediatR;
using System.Security.Claims;

namespace AngularCrudApi.Application.Pipeline.Commands
{
    public class ExportPackageCommand : BaseAction<StreamWithName>, IRequest<StreamWithName>
    {
        public ExportPackageCommand(int[] requestsId, ClaimsPrincipal user) : base(user, null)
        {
            this.RequestsId = requestsId;
        }


        public int[] RequestsId { get; }
    }
}
