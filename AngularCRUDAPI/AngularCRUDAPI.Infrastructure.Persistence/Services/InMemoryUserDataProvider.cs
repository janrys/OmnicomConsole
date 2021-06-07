using AngularCrudApi.Application.DTOs;
using AngularCrudApi.Application.Interfaces;
using AngularCrudApi.Application.Security;
using AngularCrudApi.Domain.Entities;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AngularCrudApi.Infrastructure.Persistence.Services
{
    public class InMemoryUserDataProvider : IUserDataProvider
    {
        private static readonly Dictionary<string, string> _mappingAdGroupToAppRoles = new Dictionary<string, string>();
        private List<CodebookUser> codebookUsers = new List<CodebookUser>();

        static InMemoryUserDataProvider()
        {
            foreach (RoleEnum role in RoleEnum.GetAll())
            {
                foreach (Guid id in role.ExternalIds)
                {
                    _mappingAdGroupToAppRoles.Add(id.ToString(), role.Name);
                }
            }
        }

        public Task<CodebookUser> GetByToken(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                return null;
            }

            JwtSecurityTokenHandler idTokenHandler = new JwtSecurityTokenHandler();

            if (idTokenHandler.CanReadToken(accessToken))
            {
                try
                {
                    JwtSecurityToken idToken = idTokenHandler.ReadToken(accessToken) as JwtSecurityToken;
                    string identifier = idToken.Claims.FirstOrDefault(c => c.Type.Equals("sub", StringComparison.InvariantCultureIgnoreCase))?.Value ?? "";

                    if (string.IsNullOrEmpty(identifier))
                    {
                        return null;
                    }

                    return this.GetByIdentifier(identifier);
                }
                catch // some malformed jwt tokens pass CanReadToken() method, but throw exception in ReadToken() method
                {
                    return this.GetByIdentifier(accessToken);
                }
            }
            else
            {
                return this.GetByIdentifier(accessToken);
            }
        }

        private Task<CodebookUser> GetByIdentifier(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                return null;
            }

            CodebookUser codebookUser = this.codebookUsers.FirstOrDefault(u => u.Identifier.Equals(identifier));
            return Task.FromResult(codebookUser);
        }

        public Task<CodebookUser> Login(CodebookUser codebookUser)
        {
            return this.UpsertUser(codebookUser, DateTime.UtcNow);
        }

        private Task<CodebookUser> UpsertUser(CodebookUser codebookUser, DateTime utcNow)
        {
            if (this.codebookUsers.Any(u => u.Identifier.Equals(codebookUser.Identifier)))
            {
                CodebookUser storedUser = this.codebookUsers.First(u => u.Identifier.Equals(codebookUser.Identifier));
                storedUser.RefreshFrom(codebookUser);
                codebookUser = storedUser;
            }
            else
            {
                this.codebookUsers.Add(codebookUser);
            }

            return Task.FromResult(codebookUser);
        }

        public async Task<CodebookUser> Login(CodeGrantResponse codeGrantResponse)
        {
            CodebookUser codebookUser = new CodebookUser();

            codebookUser.AccessToken = codeGrantResponse.AccessToken;
            codebookUser.RefreshToken = codeGrantResponse.RefreshToken;
            codebookUser.IdToken = codeGrantResponse.IdToken;

            JwtSecurityTokenHandler accessTokenHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken accessToken = accessTokenHandler.ReadToken(codebookUser.AccessToken) as JwtSecurityToken;

            JwtSecurityTokenHandler idTokenHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken idToken = idTokenHandler.ReadToken(codebookUser.IdToken) as JwtSecurityToken;

            codebookUser.AccessTokenExpiration = accessToken.ValidTo;
            codebookUser.RefreshTokenExpiration = DateTime.Now.AddMinutes(-1).AddDays(90); // AAD default minus approx time to process request
            codebookUser.IdTokenExpiration = idToken.ValidTo;
            codebookUser.Identifier = idToken.Claims.FirstOrDefault(c => c.Type.Equals("sub", StringComparison.InvariantCultureIgnoreCase))?.Value ?? "";

            GraphServiceClient graphServiceClient = GetGraphClient(codebookUser.AccessToken);
            User userInfo = await graphServiceClient.Me.Request().GetAsync();

            codebookUser.Upn = userInfo.UserPrincipalName ?? userInfo.OnPremisesUserPrincipalName;
            if (string.IsNullOrEmpty(codebookUser.Upn))
            {
                codebookUser.Upn = accessToken.Claims.FirstOrDefault(c => c.Type.Equals("unique_name", StringComparison.InvariantCultureIgnoreCase))?.Value ?? "";
            }

            codebookUser.Name = userInfo.DisplayName;
            if (string.IsNullOrEmpty(codebookUser.Name))
            {
                codebookUser.Name = accessToken.Claims.FirstOrDefault(c => c.Type.Equals("name", StringComparison.InvariantCultureIgnoreCase))?.Value ?? "";
            }

            codebookUser.ExternalId = userInfo.Id;

            IDirectoryObjectGetMemberGroupsCollectionPage groups = await graphServiceClient.Me.GetMemberGroups(true).Request().PostAsync();
            codebookUser.Roles.AddRange(GetRoles(groups.Select(s => s)));

            // temporary add admin role, remove after creation of AD groups for app
            //AddFakeAdminRoles(catalogUser);

            await this.Login(codebookUser);
            return codebookUser;
        }

        public async Task<CodebookUser> LoginRefresh(CodeGrantResponse codeGrantResponse, string oldAccessToken)
        {
            CodebookUser codebookUser = await this.GetByToken(oldAccessToken);

            if (codebookUser == null)
            {
                return null;
            }

            return await this.Login(codeGrantResponse);
        }

        public Task Logout(string identifier)
        {
            CodebookUser storedUser = this.codebookUsers.FirstOrDefault(u => u.Identifier.Equals(identifier));

            if (storedUser != null)
            {
                this.codebookUsers.Remove(storedUser);
            }

            return Task.CompletedTask;
        }

        private static GraphServiceClient GetGraphClient(string accessToken)
        {
            DelegateAuthenticationProvider authenticationProvider = new DelegateAuthenticationProvider(
                (request) =>
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
                    return Task.CompletedTask;
                });
            return new GraphServiceClient(authenticationProvider);
        }
        private static List<string> GetRoles(IEnumerable<string> groupIds)
        {
            List<string> roles = new List<string>();
            roles.Add(RoleEnum.Guest.Name);

            if (groupIds != null)
            {
                foreach (string groupId in groupIds)
                {
                    if (_mappingAdGroupToAppRoles.ContainsKey(groupId))
                    {
                        roles.Add(_mappingAdGroupToAppRoles[groupId]);
                    }
                }
            }

            return roles.Distinct().ToList();
        }
    }
}
