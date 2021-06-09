using AngularCrudApi.Application.DTOs;
using AngularCrudApi.Domain.Entities;
using AngularCrudApi.Infrastructure.Persistence.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularCrudApi.Application.Interfaces
{
    public interface IPackageManager
    {
        Task<StreamWithName> ExportChanges(PackageContent content);
        Task<PackageContentWithCommands> ImportChanges(StreamWithName package);
    }
}
