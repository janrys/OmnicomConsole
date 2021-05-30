using System.Collections.Generic;

namespace AngularCrudApi.Domain.Entities
{
    public class CodebookRecordChanges : CodebookDetail
    {
        public List<RecordChange> Changes { get; set; } = new List<RecordChange>();

        public CodebookRecordChanges(CodebookDetail codebookDetail)
        {
            this.Name = codebookDetail.Name;
            this.Scheme = codebookDetail.Scheme;
            this.Columns = codebookDetail.Columns;
            this.IsEditable = codebookDetail.IsEditable;
        }
    }
}
