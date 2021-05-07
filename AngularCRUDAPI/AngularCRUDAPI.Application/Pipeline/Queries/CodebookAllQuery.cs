using AngularCrudApi.Application.Interfaces;
using AngularCrudApi.Domain.Entities;
using AngularCrudApi.Domain.Security;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AngularCrudApi.Application.Pipeline.Queries
{
    public class CodebookAllQuery : BaseAction<IEnumerable<Codebook>>, IRequest<IEnumerable<Codebook>>
    {
        public CodebookAllQuery(bool includeRds, ClaimsPrincipal user) : base(user, null)
        {
            this.IncludeRds = includeRds;
        }

        public bool IncludeRds { get; }
    }
}
