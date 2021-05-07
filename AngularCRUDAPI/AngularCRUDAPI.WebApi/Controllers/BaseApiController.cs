using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AngularCrudApi.WebApi.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
        public BaseApiController(IMediator mediator)
        {
            this.Mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
        }        

        public IMediator Mediator { get; private set; }
    }
}