using Playnite.Common;
using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Serialization = Playnite.SDK.Data.Serialization;

namespace Playnite.Services
{
    public class BaseServicesClient
    {
        private static ILogger logger = LogManager.GetLogger();

        public readonly string Endpoint;
        private readonly string playniteVersionHeaderValue;

        public HttpClient HttpClient = new HttpClient()
        {
            Timeout = new TimeSpan(0, 0, 60)
        };

        public BaseServicesClient(string endpoint, Version playniteVersion)
        {
            Endpoint = endpoint.TrimEnd('/');
            playniteVersionHeaderValue = playniteVersion.ToString(4);
            HttpClient.DefaultRequestHeaders.Add("Playnite-Version", playniteVersionHeaderValue);
        }

        public T ExecuteGetRequest<T>(string subUrl)
        {
            var url = Uri.EscapeUriString(Endpoint + subUrl);
            var strResult = ExecuteRequest(url, "GET", null, null);
            var result = Serialization.FromJson<ServicesResponse<T>>(strResult);

            if (!string.IsNullOrEmpty(result.Error))
            {
                logger.Error("Service request error by proxy: " + result.Error);
                throw new Exception(result.Error);
            }

            return result.Data;
        }

        public T ExecutePostRequest<T>(string subUrl, string jsonContent)
        {
            var url = Uri.EscapeUriString(Endpoint + subUrl);
            var strResult = ExecuteRequest(url, "POST", "application/json; charset=utf-8", jsonContent);
            var result = Serialization.FromJson<ServicesResponse<T>>(strResult);

            if (!string.IsNullOrEmpty(result.Error))
            {
                logger.Error("Service request error by proxy: " + result.Error);
                throw new Exception(result.Error);
            }

            return result.Data;
        }

        protected HttpWebRequest CreateRequest(string url, string method)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;
            request.Timeout = (int)HttpClient.Timeout.TotalMilliseconds;
            request.ReadWriteTimeout = request.Timeout;
            request.UserAgent = "Playnite";
            request.Accept = "application/json";
            request.Headers.Add("Playnite-Version", playniteVersionHeaderValue);
            return request;
        }

        protected string ExecuteRequest(string url, string method, string contentType, string body)
        {
            var request = CreateRequest(url, method);

            if (body != null)
            {
                request.ContentType = contentType ?? "application/octet-stream";
                var bodyBytes = Encoding.UTF8.GetBytes(body);
                request.ContentLength = bodyBytes.Length;
                using (var reqStream = request.GetRequestStream())
                {
                    reqStream.Write(bodyBytes, 0, bodyBytes.Length);
                }
            }

            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (WebException ex) when (ex.Response is HttpWebResponse errorResponse)
            {
                using (errorResponse)
                using (var stream = errorResponse.GetResponseStream())
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
