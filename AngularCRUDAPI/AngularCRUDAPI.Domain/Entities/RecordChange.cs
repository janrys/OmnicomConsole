using System.Collections.Generic;

namespace AngularCrudApi.Domain.Entities
{
    public class RecordChange
    {
        public string Operation { get; set; }
        public KeyValuePair<string, object>? RecordKey  { get; set; }
        public Dictionary<string, object> RecordChanges { get; set; } = new Dictionary<string, object>();
    }
}
