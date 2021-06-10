using AngularCrudApi.Domain.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Exceptions;
using System;

namespace AngularCrudApi.WebApi
{
    public static class Program
    {
        private static string environmentName;

        public static void SetEnvironment()
        {
            environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (String.IsNullOrEmpty(environmentName))
            {
                Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
                environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            }
        }

        public static void Main(string[] args)
        {
            SetEnvironment();

            //Read Configuration from appSettings
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environmentName ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: false)
                .AddEnvironmentVariables()
                .Build();

            //Initialize Logger
            Log.Logger = CreateLogger(config);

            var host = CreateHostBuilder(args, config).Build();
            using (IServiceScope scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var loggerFactory = services.GetRequiredService<ILoggerFactory>();
                try
                {
                    Log.Information("Application Starting, environemnt {0}", environmentName);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "An error occurred starting the application");
                }
                finally
                {
                    Log.CloseAndFlush();
                }
            }
            host.Run();
        }

        private static Serilog.ILogger CreateLogger(IConfiguration config)
        {
            string applicatinInsightsInstrumentationKey = config.GetValue<string>($"{GlobalSettings.CONFIGURATION_KEY}:ApplicationInsightsInstrumentationKey");

            return new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .Enrich.WithExceptionDetails()
                .Destructure.ToMaximumDepth(5).Destructure.ToMaximumStringLength(1000).Destructure.ToMaximumCollectionCount(10)
                .WriteTo.Console()
                .WriteTo.ApplicationInsights(applicatinInsightsInstrumentationKey, TelemetryConverter.Traces)          
#if DEBUG
                .WriteTo.Debug()
#endif
                .CreateLogger();
        }

        public static IHostBuilder CreateHostBuilder(string[] args, IConfiguration config) =>
            Host.CreateDefaultBuilder(args)
            .UseSerilog() //Uses Serilog instead of default .NET Logger
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseEnvironment(environmentName);
                webBuilder.UseConfiguration(config);
                webBuilder.UseStartup<Startup>();
            });
    }
}