using System;

namespace AngularCrudApi.Domain.Entities
{
    public class Request
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int SequenceNumber { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public int ReleaseId { get; set; }
        public Boolean WasExported { get; set; }
        public Release Release { get; set; }

    }
}
