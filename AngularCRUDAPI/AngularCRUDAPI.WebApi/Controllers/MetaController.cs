using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AngularCrudApi.WebApi.Controllers
{
    public class MetaController : BaseApiController
    {
        public MetaController(IMediator mediator) : base(mediator)
        {
        }

        [HttpGet("/info")]
        public ActionResult<string> Info()
        {
            var assembly = typeof(Startup).Assembly;

            var lastUpdate = System.IO.File.GetLastWriteTime(assembly.Location);
            var version = FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;

            return Ok($"Version: {version}, Last Updated: {lastUpdate}");
        }
    }
}