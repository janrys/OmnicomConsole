using AngularCrudApi.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

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
