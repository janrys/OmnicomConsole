using AngularCrudApi.Domain.Entities;
using System;
using System.Collections.Generic;

namespace AngularCrudApi.Infrastructure.Persistence.Services
{
    public class PackageContent
    {
        public int PackageNumber { get; set; }
        public DateTime ExportDate { get; set; }
        public string Author { get; set; }
        public IEnumerable<Request> Requests { get; set; }
        public IEnumerable<RequestChange> RequestChanges { get; set; }

        public List<String> Files { get; set; }
    }

    public class PackageContentWithCommands : PackageContent
    {
        public PackageContentWithCommands()
        {

        }

        public PackageContentWithCommands(PackageContent packageContent)
        {
            this.PackageNumber = packageContent.PackageNumber;
            this.ExportDate = packageContent.ExportDate;
            this.Author = packageContent.Author;
            this.Requests = packageContent.Requests;
            this.RequestChanges = packageContent.RequestChanges;
            this.Files = packageContent.Files;
        }

        public string Commands { get; set; }
    }
}
