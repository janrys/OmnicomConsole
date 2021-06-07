using AngularCrudApi.Application.DTOs;
using AngularCrudApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularCrudApi.Application.Interfaces
{
    public interface IPackageManager
    {
        Task<StreamWithName> ExportChanges(int lastPackageNumber, IEnumerable<Request> requests, IEnumerable<RequestChange> requestChanges);
    }
}
