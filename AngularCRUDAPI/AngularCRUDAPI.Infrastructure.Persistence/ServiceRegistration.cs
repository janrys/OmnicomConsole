﻿using AngularCrudApi.Application.Interfaces;
using AngularCrudApi.Application.Interfaces.Repositories;
using AngularCrudApi.Infrastructure.Persistence.Contexts;
using AngularCrudApi.Infrastructure.Persistence.Repositories;
using AngularCrudApi.Infrastructure.Persistence.Repository;
using AngularCrudApi.Infrastructure.Persistence.Services;
using AngularCrudApi.Infrastructure.Persistence.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AngularCrudApi.Infrastructure.Persistence
{
    public static class ServiceRegistration
    {
        public static void AddPersistenceInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            bool useInMemory = true;
            if (useInMemory || configuration.GetValue<bool>("UseInMemoryDatabase"))
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase("ApplicationDb"));
            }
            else
            {
                services.AddDbContext<ApplicationDbContext>(options =>
               options.UseSqlServer(
                   configuration.GetConnectionString("DefaultConnection"),
                   b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));
            }


            services.Configure<AuthorizationServerSettings>(configuration.GetSection(AuthorizationServerSettings.CONFIGURATION_KEY));
            services.Configure<SqlDatabaseSettings>(configuration.GetSection(SqlDatabaseSettings.CONFIGURATION_KEY));

            services.AddTransient(typeof(IGenericRepositoryAsync<>), typeof(GenericRepositoryAsync<>));
            services.AddTransient<IPositionRepositoryAsync, PositionRepositoryAsync>();
            services.AddTransient<IEmployeeRepositoryAsync, EmployeeRepositoryAsync>();
            services.AddSingleton<IAuthorizationServerClient, AadAuthorizationServerClient>();
            services.AddSingleton<IUserDataProvider, InMemoryUserDataProvider>();
            services.AddSingleton<ICommandFactory, SqlServerCommmandFactory>();
            services.AddSingleton<ICodebookRepository, SqlDatabaseCodebookRepository>();
            services.AddSingleton<IPackageManager, InMemoryFilePackageManager>();
        }
    }
}