using AngularCrudApi.Application.Exceptions;
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
    public class CodebookApplyChangesCommandHandler : IRequestHandler<CodebookApplyChangesCommand, Unit>
    {
        private readonly ICodebookRepository codebookRepository;
        private readonly ILogger<CodebookApplyChangesCommandHandler> log;

        public CodebookApplyChangesCommandHandler(ICodebookRepository codebookRepository, ILogger<CodebookApplyChangesCommandHandler> log)
        {
            this.codebookRepository = codebookRepository ?? throw new ArgumentNullException(nameof(codebookRepository));
            this.log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task<Unit> Handle(CodebookApplyChangesCommand request, CancellationToken cancellationToken)
        {
            try
            {
                CodebookDetail codebookDetail = await this.codebookRepository.GetByName(request.CodebookName);

                if (codebookDetail == null)
                {
                    throw new ValidationException($"Codebook with name {request.CodebookName} not found");
                }

                if (request.RecordChanges == null || !request.RecordChanges.Any())
                {
                    return Unit.Value;
                }

                IEnumerable<string> wrongOperations = request.RecordChanges
                .Where(c => !RecordChangeOperationEnum.GetAll().Any(o => o.Name.Equals(c.Operation, StringComparison.InvariantCultureIgnoreCase)))
                .Select(c => c.Operation);
                if (wrongOperations.Any())
                {
                    throw new ValidationException($"Wrong operation values: {String.Join(", ", wrongOperations)} ");
                }

                CodebookRecordChanges codebookRecordChanges = new CodebookRecordChanges(codebookDetail);
                codebookRecordChanges.Changes.AddRange(request.RecordChanges);

                await this.codebookRepository.ApplyChanges(codebookRecordChanges);
                return Unit.Value;
            }
            catch (Exception exception)
            {
                this.log.LogError($"Error changing codebook {request.CodebookName} data", exception);
                throw;
            }
        }
    }
}
