using AngularCrudApi.Application.DTOs;
using AngularCrudApi.Application.Pipeline.Commands;
using AngularCrudApi.Application.Pipeline.Queries;
using AngularCrudApi.Domain.Entities;
using MediatR;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AngularCrudApi.WebApi.Extensions
{
    public class ActionBuilder : ICommandBuilder, IQueryBuilder
        , ICodeboookCommandBuilder, ICodebookQueryBuilder
        , IReleaseCommandBuilder, IReleasesQueryBuilder
        , IUserCommandBuilder, IUserQueryBuilder
    {
        private readonly ClaimsPrincipal user;
        private readonly string clientIp;
        private readonly string correlationId;
        private readonly IMediator mediator;

        public ActionBuilder(ClaimsPrincipal user, string clientIp, string correlationId, IMediator mediator)
        {
            this.user = user;
            this.clientIp = clientIp;
            this.correlationId = correlationId;
            this.mediator = mediator;
        }

        ICodeboookCommandBuilder ICommandBuilder.Codebook => this;
        IUserCommandBuilder ICommandBuilder.User => this;
        IReleaseCommandBuilder ICommandBuilder.Release => this;

        ICodebookQueryBuilder IQueryBuilder.Codebook => this;
        IReleasesQueryBuilder IQueryBuilder.Release => this;
        IUserQueryBuilder IQueryBuilder.User => this;



        Task<IEnumerable<Codebook>> ICodebookQueryBuilder.All(bool includeRds)
            => this.mediator.Send(new CodebookAllQuery(includeRds, this.user));

        Task<CodebookDetail> ICodebookQueryBuilder.ByName(string codebookName)
            => this.mediator.Send(new CodebookByNameQuery(codebookName, this.user));

        Task<CodebookDetailWithData> ICodebookQueryBuilder.Data(string codebookName)
            => this.mediator.Send(new CodebookDataQuery(codebookName, this.user));

        Task<LockState> ICodebookQueryBuilder.Lock()
            => this.mediator.Send(new LockStateQuery(this.user));

        Task<LockState> ICodeboookCommandBuilder.Lock()
            => this.mediator.Send(new CreateLockCommand(this.user));

        Task<LockState> ICodeboookCommandBuilder.Unlock()
            => this.mediator.Send(new ReleaseLockCommand(this.user));

        Task ICodeboookCommandBuilder.ApplyChanges(string codebookName, IEnumerable<RecordChange> recordChanges)
            => this.mediator.Send(new CodebookApplyChangesCommand(codebookName, recordChanges, this.user));

        Task<CodebookUser> IUserCommandBuilder.Login(CodeGrantResponse codeGrantResponse)
            => this.mediator.Send(new UserLoginCommand(codeGrantResponse, this.user));

        Task<CodebookUser> IUserCommandBuilder.Login(CodeGrantResponse codeGrantResponse, string token)
            => this.mediator.Send(new UserLoginRefreshCommand(codeGrantResponse, token, this.user));

        Task<CodebookUser> IUserQueryBuilder.ByToken(string token)
            => this.mediator.Send(new UserByTokenQuery(token, this.user));

        Task IUserCommandBuilder.Logout()
            => this.mediator.Send(new UserLogoutCommand(this.user));

        Task<IEnumerable<Release>> IReleasesQueryBuilder.All()
            => this.mediator.Send(new ReleaseAllQuery(this.user));
        Task<Release> IReleasesQueryBuilder.ById(int id)
            => this.mediator.Send(new ReleaseByIdQuery(id, this.user));

        Task<IEnumerable<Request>> IReleasesQueryBuilder.Requests(int releaseId)
            => this.mediator.Send(new RequestByReleaseQuery(releaseId, this.user));

        Task<Request> IReleasesQueryBuilder.RequestById(int id)
            => this.mediator.Send(new RequestByIdQuery(id, this.user));

        Task<Release> IReleaseCommandBuilder.Create(Release release)
            => this.mediator.Send(new ReleaseCreateCommand(release, this.user));
        Task<Release> IReleaseCommandBuilder.Update(Release release)
            => this.mediator.Send(new ReleaseUpdateCommand(release, this.user));

        Task IReleaseCommandBuilder.Delete(int id)
            => this.mediator.Send(new ReleaseDeleteCommand(id, this.user));

        Task<Request> IReleaseCommandBuilder.CreateRequest(Request request)
            => this.mediator.Send(new RequestCreateCommand(request, this.user));

        Task<Request> IReleaseCommandBuilder.UpdateRequest(Request request)
            => this.mediator.Send(new RequestUpdateCommand(request, this.user));

        Task IReleaseCommandBuilder.DeleteRequest(int requestId)
            => this.mediator.Send(new RequestDeleteCommand(requestId, this.user));
    }

    public interface IQueryBuilder
    {
        ICodebookQueryBuilder Codebook { get; }
        IReleasesQueryBuilder Release { get; }
        IUserQueryBuilder User { get; }
    }

    public interface ICommandBuilder
    {
        ICodeboookCommandBuilder Codebook { get; }
        IUserCommandBuilder User { get; }
        IReleaseCommandBuilder Release { get; }
    }

    public interface ICodeboookCommandBuilder
    {
        Task<LockState> Lock();
        Task<LockState> Unlock();
        Task ApplyChanges(string codebookName, IEnumerable<RecordChange> recordChanges);
    }
    public interface IUserCommandBuilder
    {
        Task<CodebookUser> Login(CodeGrantResponse codeGrantResponse);
        Task<CodebookUser> Login(CodeGrantResponse codeGrantResponse, string token);
        Task Logout();
    }

    public interface IReleaseCommandBuilder
    {
        Task<Release> Create(Release release);
        Task<Release> Update(Release release);
        Task Delete(int id);
        Task<Request> CreateRequest(Request request);
        Task<Request> UpdateRequest(Request request);
        Task DeleteRequest(int requestId);
    }

    public interface ICodebookQueryBuilder
    {
        Task<IEnumerable<Codebook>> All(bool includeRds = false);
        Task<CodebookDetail> ByName(string codebookName);
        Task<CodebookDetailWithData> Data(string codebookName);
        Task<LockState> Lock();
    }

    public interface IReleasesQueryBuilder
    {
        Task<IEnumerable<Release>> All();
        Task<IEnumerable<Request>> Requests(int releaseId);
        Task<Request> RequestById(int id);
        Task<Release> ById(int id);
    }

    public interface IUserQueryBuilder
    {
        Task<CodebookUser> ByToken(string token);
    }

    
}
