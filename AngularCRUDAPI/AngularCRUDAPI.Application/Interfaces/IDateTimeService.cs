using System;

namespace AngularCrudApi.Application.Interfaces
{
    public interface IDateTimeService
    {
        DateTime UtcNow { get; }
        DateTime Now { get; }
    }
}