using AngularCrudApi.Domain.Entities;
using System;
using System.Collections.Generic;

namespace AngularCrudApi.Application.DTOs
{


    public class PackageContent : PackageInfo
    {

        public DateTime ExportDate { get; set; }
        public string Author { get; set; }
        public IEnumerable<Request> Requests { get; set; }
        public IEnumerable<RequestChange> RequestChanges { get; set; }

        public List<string> Files { get; set; } = new List<string>();
    }
}
