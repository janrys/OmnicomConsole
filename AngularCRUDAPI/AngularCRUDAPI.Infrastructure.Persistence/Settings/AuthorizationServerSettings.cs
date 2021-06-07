using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularCrudApi.Infrastructure.Persistence.Settings
{
    public class AuthorizationServerSettings
    {
        public const string CONFIGURATION_KEY = "AuthorizationServerSettings";
        public string TenantId { get; set; }
        public string ClientId { get; set; }
        public string RedirectUri { get; set; }
        public string ClientSecret { get; set; }
        public string Scope { get; set; }
        public string ServerUrl { get; set; }
        public string ServerMetadataUrl { get; set; }
    }

    public static class AuthorizationServerSettingsExtension
    {
        public static string CompileMetadataUrl(this AuthorizationServerSettings settings)
        {
            return string.Format(System.Globalization.CultureInfo.InvariantCulture, settings.ServerMetadataUrl, settings.TenantId);
        }

        public static string CompileServerUrl(this AuthorizationServerSettings settings)
        {
            return string.Format(System.Globalization.CultureInfo.InvariantCulture, settings.ServerUrl, settings.TenantId);
        }
    }
}
