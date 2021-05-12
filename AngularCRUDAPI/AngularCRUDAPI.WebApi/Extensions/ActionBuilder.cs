using AngularCrudApi.Application.Pipeline.Commands;
using AngularCrudApi.Application.Pipeline.Queries;
using AngularCrudApi.Domain.Entities;
using MediatR;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AngularCrudApi.WebApi.Extensions
{
    public class ActionBuilder : ICommandBuilder, IQueryBuilder, ICodeboookCommandBuilder, ICodebookQueryBuilder, IReleasesQueryBuilder
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

        ICodebookQueryBuilder IQueryBuilder.Codebook => this;
        IReleasesQueryBuilder IQueryBuilder.Release => this;



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


        Task<IEnumerable<Release>> IReleasesQueryBuilder.All()
            => this.mediator.Send(new ReleaseAllQuery(this.user));

        Task<IEnumerable<Request>> IReleasesQueryBuilder.Requests(int releaseId)
            => this.mediator.Send(new RequestByReleaseQuery(releaseId, this.user));
    }

    public interface IQueryBuilder
    {
        ICodebookQueryBuilder Codebook { get; }
        IReleasesQueryBuilder Release { get; }
    }

    public interface ICommandBuilder
    {
        ICodeboookCommandBuilder Codebook { get; }
    }

    public interface ICodeboookCommandBuilder
    {
        Task<LockState> Lock();
        Task<LockState> Unlock();
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
    }
}
