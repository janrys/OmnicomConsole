using AngularCrudApi.Application.DTOs;
using AngularCrudApi.Application.Exceptions;
using AngularCrudApi.Application.Helpers;
using AngularCrudApi.Application.Interfaces;
using AngularCrudApi.Application.Interfaces.Repositories;
using AngularCrudApi.Application.Pipeline.Commands;
using AngularCrudApi.Application.Pipeline.Queries;
using AngularCrudApi.Domain.Entities;
using AngularCrudApi.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AngularCrudApi.Application.Pipeline.Handlers
{
    public partial class RequestCreateCommandHandler
    {
        public class PackageCommandHandler : IRequestHandler<ImportPackageCommand, Unit>
        , IRequestHandler<ExportPackageCommand, StreamWithName>
        , IRequestHandler<LastPackageQuery, PackageInfo>

        {
            private readonly ICodebookRepository codebookRepository;
            private readonly IPackageManager packageManager;
            private readonly ILogger<PackageCommandHandler> log;

            public PackageCommandHandler(ICodebookRepository codebookRepository, IPackageManager packageManager, ILogger<PackageCommandHandler> log)
            {
                this.codebookRepository = codebookRepository ?? throw new ArgumentNullException(nameof(codebookRepository));
                this.packageManager = packageManager ?? throw new ArgumentNullException(nameof(packageManager));
                this.log = log ?? throw new ArgumentNullException(nameof(log));
            }

            public async Task<Unit> Handle(ImportPackageCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    PackageContentWithCommands packageContent = await this.packageManager.ImportChanges(new StreamWithName(request.Stream, request.FileName));

                    if (packageContent == null)
                    {
                        throw new Exception("Error reading package file");
                    }

                    int lastPackageNumber = await this.codebookRepository.GetLastImportedPackageNumber();

                    if (lastPackageNumber != packageContent.PackageNumber - 1)
                    {
                        throw new ValidationException($"Wrong package number {packageContent.PackageNumber}. Expected {lastPackageNumber + 1}");
                    }

                    if (packageContent.Requests == null || !packageContent.Requests.Any())
                    {
                        throw new ValidationException($"Missing list of requests");
                    }

                    if (String.IsNullOrEmpty(packageContent.Commands))
                    {
                        throw new ValidationException($"No commands to import");
                    }

                    await this.ImportReleases(packageContent.Requests.Select(r => r.Release).Distinct());
                    await this.ImportRequests(packageContent.Requests);
                    await this.ImportRequestChanges(packageContent.RequestChanges);
                    await this.ImportCommands(packageContent.Commands);
                    await this.codebookRepository.SaveLastImportedPackageNumber(packageContent.PackageNumber);

                    return Unit.Value;
                }
                catch (Exception exception)
                {
                    this.log.LogError($"Error importing package {request.FileName}", exception);
                    throw;
                }
            }

            private Task ImportCommands(string commands)
            {
                throw new NotImplementedException();
            }

            private Task ImportRequestChanges(IEnumerable<RequestChange> requestChanges)
            {
                throw new NotImplementedException();
            }

            private Task ImportRequests(IEnumerable<Request> requests)
            {
                throw new NotImplementedException();
            }

            private Task ImportReleases(IEnumerable<Release> releases)
            {
                throw new NotImplementedException();
            }

            public async Task<StreamWithName> Handle(ExportPackageCommand request, CancellationToken cancellationToken)
            {
                if (request.RequestsId == null || !request.RequestsId.Any())
                {
                    throw new ArgumentOutOfRangeException(nameof(request.RequestsId), "List of request ids is empty");
                }

                try
                {
                    IEnumerable<Request> requests = await this.codebookRepository.GetRequestById(request.RequestsId);
                    IEnumerable<Release> releases = await this.codebookRepository.GetReleaseById(requests.Select(r => r.ReleaseId).ToArray());

                    foreach (int requestId in request.RequestsId)
                    {
                        Request storedRequest = requests.FirstOrDefault(r => r.Id == requestId);

                        if (storedRequest == null)
                        {
                            throw new ValidationException($"Request id {requestId} is not in database");
                        }

                        storedRequest.Release = releases.FirstOrDefault(r => r.Id == storedRequest.ReleaseId) ?? throw new Exception($"Release with id {storedRequest.ReleaseId} not found");
                    }

                    IEnumerable<RequestChange> requestChanges = await this.codebookRepository.GetRequestChanges(request.RequestsId);
                    int lastPackageNumber = await this.codebookRepository.GetLastExportedPackageNumber();
                    lastPackageNumber++;
                    PackageContent packageContent = new PackageContent()
                    {
                        ExportDate = DateTime.UtcNow,
                        Author = request.User.GetName(),
                        PackageNumber = lastPackageNumber,
                        Requests = requests,
                        RequestChanges = requestChanges
                    };
                    StreamWithName fileStream = await this.packageManager.ExportChanges(packageContent);
                    await this.codebookRepository.SaveLastExportedPackageNumber(lastPackageNumber);
                    await this.codebookRepository.UpdateRequestState(RequestStateEnum.Exported, request.RequestsId);
                    return fileStream;
                }
                catch (Exception exception)
                {
                    this.log.LogError($"Error exporting package for requests {String.Join(", ", request.RequestsId)}", exception);
                    throw;
                }
            }

            public async Task<PackageInfo> Handle(LastPackageQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    int lastPackageNumber = await this.codebookRepository.GetLastImportedPackageNumber();

                    return new PackageInfo() { PackageNumber = lastPackageNumber };
                }
                catch(Exception exception)
                {
                    this.log.LogError($"Error loading last package info", exception);
                    throw;
                }
            }
        }
    }
}
