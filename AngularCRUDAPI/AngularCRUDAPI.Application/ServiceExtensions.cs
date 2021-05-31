using AngularCrudApi.Application.Helpers;
using AngularCrudApi.Application.Interfaces;
using AngularCrudApi.Application.Pipeline;
using AngularCrudApi.Application.Security;
using AngularCrudApi.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AngularCrudApi.Application
{
    public static class ServiceExtensions
    {
        public static void AddApplicationLayer(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.AddSingleton<IActionAuthorizer, ClassMapActionAuthorizer>();
            services.AddMediatR(Assembly.GetExecutingAssembly());
            services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(PipelineAuthorization<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddScoped<IDataShapeHelper<Position>, DataShapeHelper<Position>>();
            services.AddScoped<IDataShapeHelper<Employee>, DataShapeHelper<Employee>>();
            services.AddScoped<IModelHelper, ModelHelper>();
            //services.AddScoped<IMockData, MockData>();
        }
    }
}