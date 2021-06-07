using MediatR;
using System.IO;
using System.Security.Claims;

namespace AngularCrudApi.Application.Pipeline.Commands
{
    public class ImportPackageCommand : BaseAction<Unit>, IRequest<Unit>
    {
        public ImportPackageCommand(Stream stream, string fileName, ClaimsPrincipal user) : base(user, null)
        {
            this.Stream = stream;
            this.FileName = fileName;
        }

        public Stream Stream { get; }
        public string FileName { get; }
    }
}
