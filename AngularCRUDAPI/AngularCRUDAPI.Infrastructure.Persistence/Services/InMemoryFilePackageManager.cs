using AngularCrudApi.Application.DTOs;
using AngularCrudApi.Application.Interfaces;
using AngularCrudApi.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularCrudApi.Infrastructure.Persistence.Services
{
    public class InMemoryFilePackageManager : IPackageManager
    {
        private readonly ILogger<InMemoryFilePackageManager> log;

        public InMemoryFilePackageManager(ILogger<InMemoryFilePackageManager> log)
        {
            this.log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task<StreamWithName> ExportChanges(int packageNumber, IEnumerable<Request> requests, IEnumerable<RequestChange> requestChanges)
        {
            MemoryStream memoryStream = new MemoryStream();

            try
            {
                using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (Request request in requests.OrderBy(r => r.SequenceNumber))
                    {
                        IEnumerable<RequestChange> requestChangesToExport = requestChanges.Where(r => r.RequestId == request.Id);

                        if (!requestChangesToExport.Any())
                        {
                            continue;
                        }

                        ZipArchiveEntry demoFile = archive.CreateEntry(this.GetSqlFileName(request.ReleaseId, request.Id, request.SequenceNumber));

                        using (Stream entryStream = demoFile.Open())
                        using (StreamWriter streamWriter = new StreamWriter(entryStream))
                        {
                            requestChangesToExport.OrderBy(r => r.SequenceNumber).ToList().ForEach(c => streamWriter.WriteLine(c.Command));
                        }
                    }
                }

                memoryStream.Seek(0, SeekOrigin.Begin);
                return new StreamWithName(memoryStream, this.GetPackageFileName(packageNumber));
            }
            catch (Exception exception)
            {
                this.log.LogError("Error exporting package", exception);
                await memoryStream.DisposeAsync();
                throw new Exception("Error exporting package", exception);
            }
        }

        private string GetPackageFileName(int packageNumber) => $"Package_Codebooks_{packageNumber}.zip";
        private string GetSqlFileName(int releaseId, int requestId, int sequenceNumber) => $"Codebooks_{releaseId}_{requestId}_{sequenceNumber}.sql";
    }
}
