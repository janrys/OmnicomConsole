using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularCrudApi.Domain.Entities
{
    public class Codebook
    {
        public string Name { get; set; }
        public string Scheme { get; set; }
        public string FullName => $"[{this.Scheme ?? "dbo"}].[{this.Name}]";

    }
}
