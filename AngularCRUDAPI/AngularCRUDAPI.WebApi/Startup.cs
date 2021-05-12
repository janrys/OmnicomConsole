using AngularCrudApi.Application;
using AngularCrudApi.Domain.Settings;
using AngularCrudApi.Infrastructure.Persistence;
using AngularCrudApi.Infrastructure.Persistence.Contexts;
using AngularCrudApi.Infrastructure.Shared;
using AngularCrudApi.WebApi.Extensions;
using AngularCrudApi.WebApi.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;

namespace AngularCrudApi.WebApi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationLayer();
            services.AddPersistenceInfrastructure(this.Configuration);
            services.AddSharedInfrastructure(this.Configuration);
            services.AddSwaggerExtension();
            services.AddControllersExtension();
            // CORS
            services.AddCorsExtension();
            services.AddHealthChecks();
            // API version
            services.AddApiVersioningExtension();
            // API explorer
            services.AddMvcCore()
                .AddApiExplorer();
            // API explorer version
            services.AddVersionedApiExplorerExtension();
            services.Configure<GlobalSettings>(this.Configuration.GetSection(GlobalSettings.CONFIGURATION_KEY));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, ApplicationDbContext dbContext, IOptions<GlobalSettings> globalConfiguration)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            dbContext.Database.EnsureCreated();
            app.InitSecuritRoles(globalConfiguration.Value.Environment);

            // Add this line; you'll need `using Serilog;` up the top, too
            app.UseSerilogRequestLogging();
            loggerFactory.AddSerilog();
            app.UseHttpsRedirection();
            app.UseRouting();
            //Enable CORS
            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSwaggerExtension();
            app.UseErrorHandlingMiddleware();
            app.UseHealthChecks("/health");

            app.UseEndpoints(endpoints =>
             {
                 endpoints.MapControllers();
             });
        }
    }
}