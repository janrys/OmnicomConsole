using AngularCrudApi.Domain.Entities;
using MediatR;
using System.Collections.Generic;
using System.Security.Claims;

namespace AngularCrudApi.Application.Pipeline.Commands
{
    public class CodebookApplyChangesCommand : BaseAction<Unit>, IRequest<Unit>
    {
        public CodebookApplyChangesCommand(string codebookName, IEnumerable<RecordChange> recordChanges, ClaimsPrincipal user) : base(user, null)
        {
            this.CodebookName = codebookName;
            this.RecordChanges = recordChanges;
        }

        public string CodebookName { get; }
        public IEnumerable<RecordChange> RecordChanges { get; }
    }
}
