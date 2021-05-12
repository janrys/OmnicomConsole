using AngularCrudApi.Application.Interfaces;
using AngularCrudApi.Domain.Settings;
using AngularCrudApi.Infrastructure.Shared.Services;
using AngularCrudApi.Infrastructure.Shared.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AngularCrudApi.Infrastructure.Shared
{
    public static class ServiceRegistration
    {
        public static void AddSharedInfrastructure(this IServiceCollection services, IConfiguration _config)
        {
            services.Configure<MailSettings>(_config.GetSection("MailSettings"));
            services.Configure<AuthorizationServerSettings>(_config.GetSection(AuthorizationServerSettings.CONFIGURATION_KEY));
            services.AddTransient<IDateTimeService, DateTimeService>();
            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<IMockService, MockService>();
            services.AddSingleton<IAuthorizationServerClient, AadAuthorizationServerClient>();
            services.AddSingleton<IUserDataProvider, InMemoryUserDataProvider>();
        }
    }
}