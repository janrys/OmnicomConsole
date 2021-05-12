using AngularCrudApi.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularCrudApi.Application.Interfaces
{
    public interface IAuthorizationServerClient
    {
        Task<CodeGrantResponse> GetToken(string code);
        Task<CodeGrantResponse> GetRefreshedToken(string accessToken, string refreshToken);
        string GetAuthorizationLink(string state);
    }
}
