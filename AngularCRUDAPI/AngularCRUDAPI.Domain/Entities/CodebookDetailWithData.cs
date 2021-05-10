using System.Collections.Generic;

namespace AngularCrudApi.Domain.Entities
{
    public class CodebookDetailWithData : CodebookDetail
    {
        public CodebookDetailWithData()
        {

        }

        public CodebookDetailWithData(CodebookDetail codebookDetail)
        {
            this.Name = codebookDetail.Name;
            this.Scheme = codebookDetail.Scheme;
            this.Columns.AddRange(codebookDetail.Columns);
        }

        public List<dynamic> Data { get; set; } = new List<object>();
    }
}
