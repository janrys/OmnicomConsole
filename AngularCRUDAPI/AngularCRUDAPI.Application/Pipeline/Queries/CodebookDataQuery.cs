using AngularCrudApi.Domain.Entities;
using MediatR;
using System.Security.Claims;

namespace AngularCrudApi.Application.Pipeline.Queries
{
    public class CodebookDataQuery : BaseAction<CodebookDetailWithData>, IRequest<CodebookDetailWithData>
    {
        public CodebookDataQuery(string codebookName, ClaimsPrincipal user) : base(user, null)
        {
            this.CodebookName = codebookName;
        }

        public string CodebookName { get; }
    }
}
