using AngularCrudApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AngularCrudApi.WebApi.Models
{
    public class RequestModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int SequenceNumber { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public int ReleaseId { get; set; }
        public Boolean WasExported { get; set; }

        public static explicit operator Request(RequestModel model)
        {
            return new Request()
            {
                Id = model.Id,
                Name = model.Name,
                SequenceNumber = model.SequenceNumber,
                Description = model.Description,
                Status = model.Status,
                ReleaseId = model.ReleaseId,
                WasExported = model.WasExported
            };
        }
    }
}
