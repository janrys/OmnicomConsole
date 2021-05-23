using AngularCrudApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularCrudApi.Application.Security
{
    public static class CodebookUserExtensions
    {
        public static CodebookUser FillFakeAdmin(this CodebookUser codebookUser)
        {
            codebookUser.ExternalId = "fakeadmin";
            codebookUser.Name = "Codebook FakeAdmin";
            codebookUser.Upn = "CodebookFakeAdmin@csas.cz";
            codebookUser.Identifier = codebookUser.Upn;
            codebookUser.Roles.AddRange(RoleEnum.GetAll().Select(r => r.Name));
            codebookUser.AccessTokenExpiration = DateTime.Now.AddHours(1);
            codebookUser.RefreshTokenExpiration = DateTime.Now.AddHours(12);
            return codebookUser;
        }

        public static CodebookUser FillFakeReader(this CodebookUser codebookUser)
        {
            codebookUser.ExternalId = "fakereader";
            codebookUser.Name = "Codebook FakeReader";
            codebookUser.Upn = "CodebookFakeReader@csas.cz";
            codebookUser.Identifier = codebookUser.Upn;
            codebookUser.Roles.Add(RoleEnum.Reader.Name);
            codebookUser.AccessTokenExpiration = DateTime.Now.AddHours(24);
            codebookUser.RefreshTokenExpiration = DateTime.Now.AddHours(30);
            return codebookUser;
        }

        public static CodebookUser FillFakeEditor(this CodebookUser codebookUser)
        {
            codebookUser.ExternalId = "fakeeditor";
            codebookUser.Name = "Codebook FakeEditor";
            codebookUser.Upn = "CodebookFakeEditor@csas.cz";
            codebookUser.Identifier = codebookUser.Upn;
            codebookUser.Roles.Add(RoleEnum.Editor.Name);
            codebookUser.AccessTokenExpiration = DateTime.Now.AddHours(1);
            codebookUser.RefreshTokenExpiration = DateTime.Now.AddHours(12);
            return codebookUser;
        }
    }
}
