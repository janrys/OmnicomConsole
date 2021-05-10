using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AngularCrudApi.Domain.Entities
{
    public static class UserExtensions
    {
        public const string AUTHENTICATION_TYPE = "OmnicomCodebookConsole";
        public const string CLAIM_TYPE_IDENTIFIER = "identifier";
        public const string CLAIM_TYPE_CEN = "cen";
        public const string CLAIM_TYPE_NAME = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
        public const string CLAIM_TYPE_ROLE = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

        public static string GetName(this ClaimsPrincipal principal)
        {
            return principal.Identities.FirstOrDefault(i => !String.IsNullOrEmpty(i.AuthenticationType) && i.AuthenticationType.Equals(AUTHENTICATION_TYPE, StringComparison.InvariantCultureIgnoreCase))?.Name ?? "";
        }

        public static string GetCen(this ClaimsPrincipal principal)
        {
            return principal.Identities
                .FirstOrDefault(i => i.AuthenticationType.Equals(AUTHENTICATION_TYPE, StringComparison.InvariantCultureIgnoreCase))
                ?.Claims?.FirstOrDefault(c => c.Type.Equals(CLAIM_TYPE_CEN, StringComparison.InvariantCultureIgnoreCase))?.Value ?? "";
        }

        public static string GetIdentifier(this ClaimsPrincipal principal)
        {
            return principal.Claims.FirstOrDefault(c => c.Type.Equals(CLAIM_TYPE_IDENTIFIER, StringComparison.InvariantCultureIgnoreCase))?.Value ?? null;
        }
    }
}
