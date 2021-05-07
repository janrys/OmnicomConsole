using AngularCrudApi.Domain.Entities;
using AngularCrudApi.WebApi.Extensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AngularCrudApi.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    public class CodebooksController : BaseApiController
    {
        private readonly ILogger<CodebooksController> log;

        public CodebooksController(IMediator mediator, ILogger<CodebooksController> log) : base(mediator)
        {
            this.log = log;
        }

        /// <summary>
        /// GET: api/controller
        /// </summary>
        /// <param name="includeRds"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool includeRds = false)
        {
            try
            {
                IEnumerable<Codebook> codeBooks = await this.Query().Codebook.All(includeRds);
                return this.Ok(codeBooks);
            }
            catch (Exception exception)
            {
                string errorMessage = "Error loading codebooks";
                this.log.LogError(errorMessage, exception);
                throw new Exception(errorMessage);
            }

        }
    }
}
