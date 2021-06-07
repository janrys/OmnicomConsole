using AngularCrudApi.Application.Interfaces;
using AngularCrudApi.Domain.Entities;
using AngularCrudApi.WebApi.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
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

        public static void AddFileNameHeadersForAngular(this HttpResponse response, string fileName)
        {
            response.Headers.Add("x-filename", System.Net.WebUtility.UrlEncode(fileName));
        }
        public static FileStreamResult FileForAngular(this BaseApiController controller, Stream fileStream, string fileName)
        {
            controller.Response.AddFileNameHeadersForAngular(fileName);
            return controller.File(fileStream, "application/octet-stream", fileName);
        }

        public static FileStreamResult FileForAngular(this BaseApiController controller, Stream fileStream, string contentType, string fileName)
        {
            controller.Response.AddFileNameHeadersForAngular(fileName);
            return controller.File(fileStream, contentType, fileName);
        }
    }

    
}
