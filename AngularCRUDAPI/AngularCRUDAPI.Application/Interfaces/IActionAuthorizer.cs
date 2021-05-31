using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularCrudApi.Application.Interfaces
{
    public interface IActionAuthorizer
    {
        Task Authorize(IAction action);
        Task<bool> CanAuthorize(IAction action);

        Task<bool> CanAuthorize(Type actionType);
    }
}
