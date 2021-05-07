using AngularCrudApi.Application.Interfaces;
using AngularCrudApi.Domain.Entities;
using AngularCrudApi.WebApi.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AngularCrudApi.WebApi.Extensions
{
    public static class ControllerExtensions
    {
        public static ICommandBuilder Command(this BaseApiController controller) => controller.Command(controller.User);
        public static ICommandBuilder CommandAnonymous(this BaseApiController controller) => controller.Command(null);

        public static ICommandBuilder Command(this BaseApiController controller, ClaimsPrincipal user)
            => new ActionBuilder(user, controller.Request.GetClientIpAddress(), controller.Request.GetCorrelationId(), controller.Mediator);

        public static IQueryBuilder Query(this BaseApiController controller) => controller.Query(controller.User);
        public static IQueryBuilder QueryAnonymous(this BaseApiController controller) => controller.Query(null);

        public static IQueryBuilder Query(this BaseApiController controller, ClaimsPrincipal user)
            => new ActionBuilder(user, controller.Request.GetClientIpAddress(), controller.Request.GetCorrelationId(), controller.Mediator);
    }

    
}
