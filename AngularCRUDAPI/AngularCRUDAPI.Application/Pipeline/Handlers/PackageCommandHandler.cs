using AngularCrudApi.Application.DTOs;
using AngularCrudApi.Application.Exceptions;
using AngularCrudApi.Application.Helpers;
using AngularCrudApi.Application.Interfaces;
using AngularCrudApi.Application.Interfaces.Repositories;
using AngularCrudApi.Application.Pipeline.Commands;
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

            public Task<Unit> Handle(ImportPackageCommand request, CancellationToken cancellationToken)
            {
                try
                {


                    return Unit.Task;
                    //return this.codebookRepository.CreateRequest(request.Request);
                }
                catch (Exception exception)
                {
                    this.log.LogError($"Error importing package {request.FileName}", exception);
                    throw;
                }
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
                    StreamWithName fileStream = await this.packageManager.ExportChanges(lastPackageNumber, requests, requestChanges);
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

        }
    }
}
