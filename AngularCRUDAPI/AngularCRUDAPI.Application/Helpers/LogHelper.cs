using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularCrudApi.Application.Helpers
{
    public static class LogHelper
    {
        public static string ToLogString(this object item) => Newtonsoft.Json.JsonConvert.SerializeObject(item);
    }
}
