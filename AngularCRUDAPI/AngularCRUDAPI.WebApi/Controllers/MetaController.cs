using AngularCrudApi.Application.Helpers;
using AngularCrudApi.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AngularCrudApi.WebApi.Controllers
{
    public class MetaController : BaseApiController
    {
        private readonly ICodebookRepository codebookRepository;

        public MetaController(IMediator mediator, ICodebookRepository codebookRepository) : base(mediator)
        {
            this.codebookRepository = codebookRepository ?? throw new System.ArgumentNullException(nameof(codebookRepository));
        }

        [HttpGet("/info")]
        public ActionResult<string> Info()
        {
            var assembly = typeof(Startup).Assembly;

            var lastUpdate = System.IO.File.GetLastWriteTime(assembly.Location);
            var version = FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;

            return Ok($"Version: {version}, Last Updated: {lastUpdate}");
        }

        [ProducesResponseType(typeof(DateTime), StatusCodes.Status200OK)]
        [HttpGet("/pingsql")]
        public async Task<IActionResult> PingSql()
        {
            try
            {
                DateTime sqlServerDatetime = await this.codebookRepository.Ping();
                return this.Ok($"Sql server OK, it's date is: {sqlServerDatetime}");
            }
            catch (Exception exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, exception.ToLogString());
            }
        }
    }
}