using AngularCrudApi.Application.Interfaces;
using AngularCrudApi.Domain.Security;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace AngularCrudApi.Application.Pipeline
{
    public abstract class BaseAuditedAction<TResult> : BaseAction<TResult>, IAuditedAction
    {
        protected BaseAuditedAction(ClaimsPrincipal user, IEnumerable<RoleEnum> allowedRoles, string clientIp, string correlationId) : this(user, allowedRoles, clientIp, correlationId, 0, "", null)
        {
        }

        protected BaseAuditedAction(ClaimsPrincipal user, IEnumerable<RoleEnum> allowedRoles, string clientIp, string correlationId, int eventId, string auditMessageTemplate, object[] auditMessagePropertyValues) : base(user, allowedRoles)
        {
            this.Time = DateTime.UtcNow;
            this.ClientIp = clientIp;
            this.CorrelationId = correlationId;
            this.EventId = eventId;
            this.AuditMessageTemplate = auditMessageTemplate;
            this.AuditMessagePropertyValues = auditMessagePropertyValues ?? new object[] { };
        }

        public DateTime Time { get; }

        public string ClientIp { get; }

        public string CorrelationId { get; }

        public int EventId { get; }

        public string AuditMessageTemplate { get; }

        public object[] AuditMessagePropertyValues { get; }
    }
}
