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
    public class CodebookDataQueryHandler : IRequestHandler<CodebookDataQuery, CodebookDetailWithData>
    {
        private readonly ICodebookRepository codebookRepository;
        private readonly ILogger<CodebookAllQueryHandler> log;

        public CodebookDataQueryHandler(ICodebookRepository codebookRepository, ILogger<CodebookAllQueryHandler> log)
        {
            this.codebookRepository = codebookRepository ?? throw new ArgumentNullException(nameof(codebookRepository));
            this.log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public Task<CodebookDetailWithData> Handle(CodebookDataQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return this.codebookRepository.GetData(request.CodebookName);
            }
            catch (Exception exception)
            {
                this.log.LogError($"Error loading data from codebook {request.CodebookName}", exception);
                throw;
            }
        }
    }
}
