using AngularCrudApi.Application.Exceptions;
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
    public class ReleasesController : BaseApiController
    {
        private readonly ILogger<ReleasesController> log;

        public ReleasesController(IMediator mediator, ILogger<ReleasesController> log) : base(mediator)
        {
            this.log = log ?? throw new ArgumentNullException(nameof(log));
        }

        /// <summary>
        /// Get list of releases
        /// </summary>
        /// <returns>Release list</returns>
        [ProducesResponseType(typeof(IEnumerable<Release>), StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                IEnumerable<Release> releases = await this.Query().Release.All();
                return this.Ok(releases);
            }
            catch (Exception exception)
            {
                string errorMessage = "Error loading releases";
                this.log.LogError(errorMessage, exception);
                throw new Exception(errorMessage);
            }
        }

        /// <summary>
        /// Get list of releases
        /// </summary>
        /// <returns>Release list</returns>
        [ProducesResponseType(typeof(IEnumerable<Request>), StatusCodes.Status200OK)]
        [HttpGet("{releaseId:int}/requests")]
        public async Task<IActionResult> GetRequests(int releaseId)
        {
            try
            {
                IEnumerable<Request> requests = await this.Query().Release.Requests(releaseId);
                return this.Ok(requests);
            }
            catch (Exception exception)
            {
                string errorMessage = "Error loading requests";
                this.log.LogError(errorMessage, exception);
                throw new Exception(errorMessage);
            }
        }
    }
}
