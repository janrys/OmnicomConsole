using AngularCrudApi.Application.DTOs;
using AngularCrudApi.Application.Interfaces;
using AngularCrudApi.Domain.Entities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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

        public async Task<StreamWithName> ExportChanges(PackageContent packageContent)
        {
            MemoryStream memoryStream = new MemoryStream();

            try
            {
                using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    int fileSequence = 0;
                    foreach (Request request in packageContent.Requests.OrderBy(r => r.SequenceNumber))
                    {
                        IEnumerable<RequestChange> requestChangesToExport = packageContent.RequestChanges.Where(r => r.RequestId == request.Id);

                        if (!requestChangesToExport.Any())
                        {
                            continue;
                        }

                        fileSequence++;
                        string sqlFileName = this.GetSqlFileName(fileSequence, request.ReleaseId, request.Id, request.SequenceNumber);
                        ZipArchiveEntry changesFile = archive.CreateEntry(sqlFileName);
                        packageContent.Files.Add(sqlFileName);

                        using (Stream entryStream = changesFile.Open())
                        using (StreamWriter streamWriter = new StreamWriter(entryStream))
                        {
                            requestChangesToExport.OrderBy(r => r.SequenceNumber).ToList().ForEach(c => streamWriter.WriteLine(c.Command));
                        }
                    }

                    ZipArchiveEntry metadatFile = archive.CreateEntry(this.GetMetadataFileName());
                    using (Stream entryStream = metadatFile.Open())
                    using (StreamWriter streamWriter = new StreamWriter(entryStream))
                    {
                        this.GetJsonSerializer().Serialize(streamWriter, packageContent);
                    }
                }

                memoryStream.Seek(0, SeekOrigin.Begin);
                return new StreamWithName(memoryStream, this.GetPackageFileName(packageContent.PackageNumber));
            }
            catch (Exception exception)
            {
                this.log.LogError("Error exporting package", exception);
                await memoryStream.DisposeAsync();
                throw new Exception("Error exporting package", exception);
            }
        }

        public async Task<PackageContentWithCommands> ImportChanges(StreamWithName package)
        {
            if (package is null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            PackageContentWithCommands packageContentWithCommands = new PackageContentWithCommands();
            packageContentWithCommands.Commands = "";
            try
            {
                using (ZipArchive archive = new ZipArchive(package.Stream, ZipArchiveMode.Read, false))
                {
                    ZipArchiveEntry metadataFile = archive.GetEntry(this.GetMetadataFileName());

                    if (metadataFile != null)
                    {
                        using (StreamReader file = new StreamReader(metadataFile.Open()))
                        {
                            PackageContent packageContent = (PackageContent)this.GetJsonSerializer().Deserialize(file, typeof(PackageContent));
                            packageContentWithCommands = new PackageContentWithCommands(packageContent);
                        }
                    }

                    if (packageContentWithCommands.Files != null && packageContentWithCommands.Files.Any())
                    {
                        foreach (string sqlFileName in packageContentWithCommands.Files)
                        {
                            ZipArchiveEntry sqlFile = archive.GetEntry(sqlFileName);

                            using (StreamReader file = new StreamReader(sqlFile.Open()))
                            {
                                packageContentWithCommands.Commands += (await file.ReadToEndAsync()) + Environment.NewLine;
                            }
                        }
                    }
                }

                return packageContentWithCommands;
            }
            catch (Exception exception)
            {
                string errorMessage = $"Error importing package {package.Name}";
                this.log.LogError(errorMessage, exception);
                throw new Exception(errorMessage, exception);
            }
        }


        private string GetPackageFileName(int packageNumber) => $"Package_Codebooks_{packageNumber}.zip";
        private string GetSqlFileName(int fileSequence, int releaseId, int requestId, int sequenceNumber) => $"Codebooks_{fileSequence}_{releaseId}_{requestId}_{sequenceNumber}.sql";
        private string GetMetadataFileName() => $"Codebooks_metadata.json";

        private JsonSerializer jsonSerializer;
        private JsonSerializer GetJsonSerializer() => this.jsonSerializer ?? (this.jsonSerializer = new JsonSerializer());

    }
}
