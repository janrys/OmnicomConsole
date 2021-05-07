using System;
using System.Security.Claims;

namespace AngularCrudApi.Application.Interfaces
{
    public interface IAuditedAction
    {
        DateTime Time { get; }
        ClaimsPrincipal User { get; }
        string ClientIp { get; }
        string CorrelationId { get; }
        int EventId { get; }

        string AuditMessageTemplate { get; }

        object[] AuditMessagePropertyValues { get; }
    }

    
}
