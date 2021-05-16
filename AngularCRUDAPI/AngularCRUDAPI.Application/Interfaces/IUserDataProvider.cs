using AngularCrudApi.Application.DTOs;
using AngularCrudApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularCrudApi.Application.Interfaces
{
    public interface IUserDataProvider
    {
        Task<CodebookUser> Login(CodebookUser codebookUser);
        Task<CodebookUser> Login(CodeGrantResponse codeGrantResponse);
        Task Logout(string identifier);
        Task<CodebookUser> LoginRefresh(CodeGrantResponse codeGrantResponse, string oldAccessToken);
        Task<CodebookUser> GetByToken(string accessToken);
    }
}
