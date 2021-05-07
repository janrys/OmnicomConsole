using System.Collections.Generic;

namespace AngularCrudApi.Domain.Entities
{
    public class CodebookDetail : Codebook
    {
        public List<ColumnDefinition> Columns { get; set; } = new List<ColumnDefinition>();
    }
}
