using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AngularCrudApi.WebApi.Extensions
{
    public static class IHostingEnvironmentExtensions
    {
        public static Boolean IsCodebookDevelopment(this IWebHostEnvironment hostingEnvironment)
        {
            return hostingEnvironment.IsDevelopment() || hostingEnvironment.IsEnvironment("DevelopmentAzure") || hostingEnvironment.IsEnvironment("Testing");
        }
    }
}
