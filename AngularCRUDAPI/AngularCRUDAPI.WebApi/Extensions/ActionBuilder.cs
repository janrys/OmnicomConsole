using AngularCrudApi.Application.Pipeline.Queries;
using AngularCrudApi.Domain.Entities;
using MediatR;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AngularCrudApi.WebApi.Extensions
{
    public class ActionBuilder : ICommandBuilder, IQueryBuilder, ICodeboookCommandBuilder, ICodebookQueryBuilder
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

        Task<IEnumerable<Codebook>> ICodebookQueryBuilder.All(bool includeRds)
            => this.mediator.Send(new CodebookAllQuery(includeRds, this.user));
    }

    public interface IQueryBuilder
    {
        ICodebookQueryBuilder Codebook { get; }
    }

    public interface ICommandBuilder
    {
        ICodeboookCommandBuilder Codebook { get; }
    }

    public interface ICodeboookCommandBuilder
    {
    }

    public interface ICodebookQueryBuilder
    {
        Task<IEnumerable<Codebook>> All(bool includeRds = false);
    }
}
