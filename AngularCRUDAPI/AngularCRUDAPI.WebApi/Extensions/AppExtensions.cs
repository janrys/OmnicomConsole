﻿using AngularCrudApi.Application.Security;
using AngularCrudApi.WebApi.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace AngularCrudApi.WebApi.Extensions
{
    public static class AppExtensions
    {
        public static void UseSwaggerExtension(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("v1/swagger.json", "Printnet.CodebooksConsole.WebApi");
            });
        }

        public static void UseErrorHandlingMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<ErrorHandlerMiddleware>();
        }

        public static IApplicationBuilder InitSecuritRoles(this IApplicationBuilder builder, string environment)
        {
            RoleGuids.SetupInstance(environment);
            return builder;
        }
    }
}