using AngularCrudApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularCrudApi.Application.Interfaces.Repositories
{
    public interface ICodebookRepository
    {
        public Task<IEnumerable<Codebook>> GetAll(bool includeRds);
        public Task<CodebookDetail> GetByName(string codebookName);
    }


}
