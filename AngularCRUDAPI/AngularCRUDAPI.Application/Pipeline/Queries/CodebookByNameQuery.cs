using AngularCrudApi.Domain.Entities;
using MediatR;
using System.Security.Claims;

namespace AngularCrudApi.Application.Pipeline.Queries
{
    public class CodebookByNameQuery : BaseAction<CodebookDetail>, IRequest<CodebookDetail>
    {
        public CodebookByNameQuery(string codebookName, ClaimsPrincipal user) : base(user, null)
        {
            this.CodebookName = codebookName;
        }

        public string CodebookName { get; }
    }
}
