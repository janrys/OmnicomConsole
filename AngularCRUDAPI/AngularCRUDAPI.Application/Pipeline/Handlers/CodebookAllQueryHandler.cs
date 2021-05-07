using AngularCrudApi.Application.Interfaces.Repositories;
using AngularCrudApi.Application.Pipeline.Queries;
using AngularCrudApi.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AngularCrudApi.Application.Pipeline.Handlers
{
    public class CodebookAllQueryHandler : IRequestHandler<CodebookAllQuery, IEnumerable<Codebook>>
    {
        private readonly ICodebookRepository codebookRepository;
        private readonly ILogger<CodebookAllQueryHandler> log;

        public CodebookAllQueryHandler(ICodebookRepository codebookRepository, ILogger<CodebookAllQueryHandler> log)
        {
            this.codebookRepository = codebookRepository ?? throw new ArgumentNullException(nameof(codebookRepository));
            this.log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public Task<IEnumerable<Codebook>> Handle(CodebookAllQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return this.codebookRepository.GetAll(request.IncludeRds);
            }
            catch(Exception exception)
            {
                this.log.LogError("Error loading codebooks", exception);
                throw;
            }
        }
    }
}
