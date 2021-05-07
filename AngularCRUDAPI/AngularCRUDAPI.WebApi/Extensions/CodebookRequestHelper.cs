using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace AngularCrudApi.WebApi.Extensions
{
    public static class CodebookRequestExtensions
    {
        public static readonly string CORRELATION_ID_HEADER_NAME = "CodebookCorrelationId";
        public static readonly string EMPTY_CORRELATION_ID = Guid.Empty.ToString();

        public static void AddCorrelationId(this IHeaderDictionary headers, string correlationId)
        {
            headers.Add(CORRELATION_ID_HEADER_NAME, new string[] { correlationId });
        }

        public static void AddCorrelationId(this HttpRequestHeaders headers, string correlationId)
        {
            headers.Add(CORRELATION_ID_HEADER_NAME, new string[] { correlationId });
        }

        public static void AddCorrelationId(this HttpContentHeaders headers, string correlationId)
        {
            headers.Add(CORRELATION_ID_HEADER_NAME, new string[] { correlationId });
        }

        public static string GetClientIpAddress(this HttpRequest request)
        {
            return request.HttpContext.Connection.RemoteIpAddress.ToString();
        }

        public static string GetCorrelationId(this HttpRequest request)
        {
            KeyValuePair<string, StringValues> header = request.Headers.FirstOrDefault(h => h.Key.Equals(CORRELATION_ID_HEADER_NAME, StringComparison.InvariantCultureIgnoreCase));

            if (!header.Value.Any())
            {
                return "";
            }

            return header.Value.First();
        }
    }
}
