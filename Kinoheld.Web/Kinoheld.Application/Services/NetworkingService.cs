using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Kinoheld.Application.Services
{
    public static class NetworkingService
    {
        private static string BuildQueryParameter(IEnumerable<KeyValuePair<string, string>> parameterList)
        {
            if (parameterList == null)
            {
                return string.Empty;
            }

            var result = new StringBuilder();
            var first = true;
            foreach (var parameter in parameterList)
            {
                if (first)
                {
                    result.Append("?");
                    first = false;
                }
                else
                {
                    result.Append("&");
                }

                var bytes = Encoding.Default.GetBytes(parameter.Key);
                var key = Encoding.UTF8.GetString(bytes);

                bytes = Encoding.Default.GetBytes(parameter.Value);
                var value = Encoding.UTF8.GetString(bytes);

                result.Append(key);
                result.Append("=");
                result.Append(value);
            }

            return result.ToString();
        }

        private static async Task<Stream> GetStreamGetAsync(string url, string authorizationHeader = null)
        {
            var request = (HttpWebRequest)WebRequest.Create(new Uri(url));
            request.Accept = "application/json";
            request.ContentType = "application/json";
            request.Method = "GET";

            if (!string.IsNullOrEmpty(authorizationHeader))
            {
                request.Headers.Add("Authorization", authorizationHeader);
            }
                
            var response = (HttpWebResponse)await request.GetResponseAsync().ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.GetResponseStream();
            }

            return null;
        }
        
        public static async Task<JObject> ReadJsonObjectFromUrlGet(string url, IEnumerable<KeyValuePair<string, string>> parameters, string authorizationHeader = null)
        {
            if (parameters != null)
            {
                var parametersStr = BuildQueryParameter(parameters);
                url = string.Concat(url, parametersStr);
            }

            using (var inputStream = await GetStreamGetAsync(url, authorizationHeader).ConfigureAwait(false))
            {
                if (inputStream == null)
                {
                    return null;
                }

                using (var sr = new StreamReader(inputStream, Encoding.UTF8))
                {
                    var jsonString = sr.ReadToEnd();
                    return JObject.Parse(jsonString);
                }
            }
        }

        public static async Task<string> ReadStringFromUrlGet(string url, IEnumerable<KeyValuePair<string, string>> parameters, string authorizationHeader = null)
        {
            if (parameters != null)
            {
                var parametersStr = BuildQueryParameter(parameters);
                url = string.Concat(url, parametersStr);
            }
            
            using (var inputStream = await GetStreamGetAsync(url, authorizationHeader).ConfigureAwait(false))
            {
                if (inputStream == null)
                {
                    return null;
                }

                using (var sr = new StreamReader(inputStream, Encoding.UTF8))
                {
                    return sr.ReadToEnd();
                }
            }
                
        }
    }
}