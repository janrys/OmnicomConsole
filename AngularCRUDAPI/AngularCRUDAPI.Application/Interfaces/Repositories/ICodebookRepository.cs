using AngularCrudApi.Domain.Entities;
using AngularCrudApi.Domain.Enums;
using MediatR;
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
        Task<LockState> CreateLock(string userIdentifier, string userName, int requestId, DateTime? created = null);
        Task<LockState> ReleaseLock(DateTime? released = null);

        Task<IEnumerable<Release>> GetAllReleases();
        Task<Release> CreateRelease(Release release);
        Task<Release> UpdateRelease(Release release);
        Task DeleteRelease(int releaseId);
        Task<IEnumerable<Request>> GetRequests(int releaseId);
        Task<Request> CreateRequest(Request request);
        Task<Request> UpdateRequest(Request request);
        Task DeleteRequest(int requestId);
        Task DeleteRequestsByReleaseId(int releaseId);
        Task<Release> GetReleaseById(int id);
        Task<DateTime> Ping();
        Task<IEnumerable<Release>> GetReleaseById(int[] ids);
        Task ApplyChanges(int requestId, CodebookRecordChanges codebookRecordChanges);
        Task<Request> GetRequestById(int id);
        Task<IEnumerable<Request>> GetRequestById(int[] requestsId);
        Task<IEnumerable<RequestChange>> GetRequestChanges(int[] requestsId);
        Task<int> GetLastExportedPackageNumber();
        Task SaveLastExportedPackageNumber(int lastPackageNumber);
        Task<int> GetLastImportedPackageNumber();
        Task SaveLastImportedPackageNumber(int lastPackageNumber);
        Task UpdateRequestState(RequestStateEnum exported, int[] requestsId);
    }


}
