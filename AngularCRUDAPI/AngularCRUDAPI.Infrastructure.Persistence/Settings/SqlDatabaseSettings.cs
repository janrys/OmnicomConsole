using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularCrudApi.Infrastructure.Persistence.Settings
{
    public class SqlDatabaseSettings
    {
        public const string CONFIGURATION_KEY = "SqlDatabaseSettings";
        public string ConnectionString { get; set; }
    }
}
