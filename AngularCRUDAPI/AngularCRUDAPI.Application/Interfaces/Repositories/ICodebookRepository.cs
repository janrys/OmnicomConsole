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

        public Task<CodebookDetailWithData> GetData(string codebookName);
        Task<LockState> GetLock();
        Task<LockState> CreateLock(string userIdentifier, string userName, int releaseId, DateTime? created = null);
        Task<LockState> ReleaseLock(DateTime? released = null);

        Task<CodebookDetailWithData> InsertData(string codebookName, int releaseId, IDictionary<string, object> values);
        Task UpdateData(string codebookName, int releaseId, object key, IDictionary<string, object> values);
        Task DeleteData(string codebookName, int releaseId, object key);
        Task<IEnumerable<Release>> GetAllReleases();
        Task<IEnumerable<Request>> GetRequests(int releaseId);
    }


}
