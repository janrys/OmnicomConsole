using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularCrudApi.Domain.Settings
{
    public class GlobalSettings
    {
        public const string CONFIGURATION_KEY = "GlobalSettings";

        public string Environment { get; set; }
        public string FrontendVersion { get; set; }
    }
}
