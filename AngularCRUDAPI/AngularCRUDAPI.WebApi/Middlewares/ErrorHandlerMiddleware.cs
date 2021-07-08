using AngularCrudApi.Application.Exceptions;
using AngularCrudApi.Application.Wrappers;
using AngularCrudApi.WebApi.Extensions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace AngularCrudApi.WebApi.Middlewares
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private const string EXPOSE_HEADER_NAME = "access-control-expose-headers";

        public ErrorHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            string correlationId = Guid.NewGuid().ToString();
            context.Request.Headers.AddCorrelationId(correlationId);
            context.Response.Headers.AddCorrelationId(correlationId);

            if (context.Response.Headers.ContainsKey(EXPOSE_HEADER_NAME))
            {
                context.Response.Headers[EXPOSE_HEADER_NAME] += $"{CodebookRequestExtensions.CORRELATION_ID_HEADER_NAME},x-filename,Content-Disposition";
            }
            else
            {
                context.Response.Headers.Add(EXPOSE_HEADER_NAME, $"{CodebookRequestExtensions.CORRELATION_ID_HEADER_NAME},x-filename,Content-Disposition");
            }

            try
            {
                await _next(context);
            }
            catch (Exception error)
            {
                var response = context.Response;
                response.ContentType = "application/json";
                var responseModel = new Response<string>() { Succeeded = false, Message = error?.Message };

                switch (error)
                {
                    case ValidationException e:
                        // custom application error
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        responseModel.Errors = e.Errors;
                        break;

                    case ForbiddenException e:
                        // custom application error
                        response.StatusCode = (int)HttpStatusCode.Forbidden;
                        responseModel.Errors = new List<string>();

                        responseModel.Errors.Add(e.Message);
                        Exception currentException = e;

                        while (currentException.InnerException != null)
                        {
                            currentException = e.InnerException;
                            responseModel.Errors.Add(currentException.Message);
                        }

                        break;

                    case ApiException e:
                        // custom application error
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        break;

                    case KeyNotFoundException e:
                        // not found error
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        break;

                    default:
                        // unhandled error
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        break;
                }
                var result = JsonSerializer.Serialize(responseModel);

                await response.WriteAsync(result);
            }
        }
    }
}