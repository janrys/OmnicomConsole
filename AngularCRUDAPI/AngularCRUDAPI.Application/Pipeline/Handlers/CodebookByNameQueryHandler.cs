using AngularCrudApi.Application.Interfaces.Repositories;
using AngularCrudApi.Application.Pipeline.Queries;
using AngularCrudApi.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AngularCrudApi.Application.Pipeline.Handlers
{
    public class CodebookByNameQueryHandler : IRequestHandler<CodebookByNameQuery, CodebookDetail>
    {
        private readonly ICodebookRepository codebookRepository;
        private readonly ILogger<CodebookAllQueryHandler> log;

        public CodebookByNameQueryHandler(ICodebookRepository codebookRepository, ILogger<CodebookAllQueryHandler> log)
        {
            this.codebookRepository = codebookRepository ?? throw new ArgumentNullException(nameof(codebookRepository));
            this.log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public Task<CodebookDetail> Handle(CodebookByNameQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return this.codebookRepository.GetByName(request.CodebookName);
            }
            catch (Exception exception)
            {
                this.log.LogError($"Error loading codebook {request.CodebookName}", exception);
                throw;
            }
        }
    }
}
