using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularCrudApi.Domain.Entities
{
    public class PublicCodebookUser
    {
        public string Identifier { get; set; }
        public string Name { get; set; }
        public string Upn { get; set; }
        public List<string> Roles { get; } = new List<string>();
        public DateTime AccessTokenExpiration { get; set; }

        public DateTime RefreshTokenExpiration { get; set; }
        public DateTime IdTokenExpiration { get; set; }
    }

    public class CodebookUser : PublicCodebookUser
    {
        public string ExternalId { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string IdToken { get; set; }
    }

    public static class CodebookUserExtensions
    {
        public static void RefreshFrom(this CodebookUser user, CodebookUser fromUser)
        {
            user.AccessToken = fromUser.AccessToken;
            user.AccessTokenExpiration = fromUser.AccessTokenExpiration;
            user.ExternalId = fromUser.ExternalId;
            user.Identifier = fromUser.Identifier;
            user.IdToken = fromUser.IdToken;
            user.IdTokenExpiration = fromUser.IdTokenExpiration;
            user.Name = fromUser.Name;
            user.RefreshToken = fromUser.RefreshToken;
            user.RefreshTokenExpiration = fromUser.RefreshTokenExpiration;
            user.Roles.Clear();
            user.Roles.AddRange(fromUser.Roles);
            user.Upn = fromUser.Upn;
        }

        public static PublicCodebookUser AsPublic(this CodebookUser codebookUser)
        {
            PublicCodebookUser publicCatalogUser = new PublicCodebookUser()
            {
                Identifier = codebookUser.Identifier,
                Name = codebookUser.Name,
                Upn = codebookUser.Upn,
                AccessTokenExpiration = codebookUser.AccessTokenExpiration,
                RefreshTokenExpiration = codebookUser.RefreshTokenExpiration,
                IdTokenExpiration = codebookUser.IdTokenExpiration
            };
            publicCatalogUser.Roles.AddRange(codebookUser.Roles);
            return publicCatalogUser;
        }        
    }
}
