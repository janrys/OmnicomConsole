using AngularCrudApi.Application.DTOs;
using AngularCrudApi.Application.Interfaces;
using AngularCrudApi.Infrastructure.Shared.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AngularCrudApi.Infrastructure.Shared.Services
{
    public class AadAuthorizationServerClient : IAuthorizationServerClient
    {
        private readonly AuthorizationServerSettings configuration;

        public AadAuthorizationServerClient(IOptions<AuthorizationServerSettings> configuration)
        {
            this.configuration = configuration.Value;

            if (this.configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
        }

        public string GetAuthorizationLink(string state)
        {
            string authorizationLink = string.Format(this.configuration.CompileServerUrl() + "/authorize", this.configuration.TenantId)
                + $"?response_type=code&response_mode=query&client_id={this.configuration.ClientId}&redirect_uri={Uri.EscapeDataString(this.configuration.RedirectUri)}&scope={Uri.EscapeDataString(this.configuration.Scope)}";

            if (!string.IsNullOrEmpty(state))
            {
                authorizationLink += $"&state={state}";
            }

            return authorizationLink;
        }

        public Task<CodeGrantResponse> GetRefreshedToken(string accessToken, string refreshToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new ArgumentNullException(nameof(accessToken));
            }

            if (string.IsNullOrEmpty(refreshToken))
            {
                throw new ArgumentNullException(nameof(refreshToken));
            }

            return this.CallTokenEndpoint(this.GetAuthorizationRequestParams("refresh_token", refreshToken: refreshToken));
        }

        public Task<CodeGrantResponse> GetToken(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException(nameof(code));
            }

            return this.CallTokenEndpoint(this.GetAuthorizationRequestParams("authorization_code", code: code));
        }

        private string GetAuthorizationRequestParams(string grantType, string refreshToken = null, string code = null )
        {
            if (string.IsNullOrEmpty(grantType))
            {
                throw new ArgumentNullException(nameof(grantType));
            }

            string requestParams = $"grant_type={grantType}&client_id={this.configuration.ClientId}&client_secret={Uri.EscapeDataString(this.configuration.ClientSecret)}&redirect_uri={Uri.EscapeDataString(this.configuration.RedirectUri)}";

            if (!string.IsNullOrEmpty(refreshToken))
            {
                return requestParams + $"&refresh_token={refreshToken}";
            }
            else if(!string.IsNullOrEmpty(code))
            {
                return requestParams + $"&code={code}";
            }
            else
            {
                throw new ArgumentNullException();
            }
        }

        private async Task<CodeGrantResponse> CallTokenEndpoint(string requestParams)
        {
            if (string.IsNullOrEmpty(requestParams))
            {
                throw new ArgumentNullException(nameof(requestParams));
            }

            string authUri = string.Format(this.configuration.CompileServerUrl() + "/token", this.configuration.TenantId);
            HttpWebRequest authRequest = WebRequest.CreateHttp(authUri);
            authRequest.AllowAutoRedirect = false;

            authRequest.Method = "POST";
            authRequest.ContentType = "application/x-www-form-urlencoded";
            string postData = requestParams;
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            authRequest.ContentLength = byteArray.Length;
            Stream dataStream = await authRequest.GetRequestStreamAsync();
            await dataStream.WriteAsync(byteArray, 0, byteArray.Length);
            dataStream.Close();

            WebResponse authResponse = null;
            try
            {
                authResponse = await authRequest.GetResponseAsync();
            }
            catch (WebException exception)
            {
                string content = await new StreamReader(exception.Response.GetResponseStream()).ReadToEndAsync();
                throw new Exception("Error from authorization server. " + content, exception);
            }

            using (Stream responseStream = authResponse.GetResponseStream())
            using (StreamReader reader = new StreamReader(responseStream))
            {
                string responseFromServer = await reader.ReadToEndAsync();
                CodeGrantResponse codeGrantResponse = null;

                try
                {
                    codeGrantResponse = JsonConvert.DeserializeObject<CodeGrantResponse>(responseFromServer);
                }
                catch (Exception exception)
                {
                    throw new Exception("Error parsing response from authorization server", exception);
                }

                if (codeGrantResponse == null
                    || string.IsNullOrEmpty(codeGrantResponse.AccessToken)
                    || string.IsNullOrEmpty(codeGrantResponse.RefreshToken))
                {
                    throw new Exception("Authorization server response does not contain access and refresh token");
                }

                return codeGrantResponse;
            }
        }
    }
}
