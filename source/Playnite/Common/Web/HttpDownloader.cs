using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Playnite.SDK;
using System.Threading;

namespace Playnite.Common.Web
{
    public class HttpDownloader
    {
        private static ILogger logger = LogManager.GetLogger();
        private static readonly Downloader downloader = new Downloader();

        public static string DownloadString(IEnumerable<string> mirrors)
        {
            return downloader.DownloadString(mirrors);
        }

        public static string DownloadString(string url)
        {
            return downloader.DownloadString(url);
        }

        public static string DownloadString(string url, CancellationToken cancelToken)
        {
            return downloader.DownloadString(url, cancelToken);
        }

        public static string DownloadString(string url, Encoding encoding)
        {
            return downloader.DownloadString(url, encoding);
        }

        public static string DownloadString(string url, List<Cookie> cookies)
        {
            return downloader.DownloadString(url, cookies);
        }

        public static string DownloadString(string url, List<Cookie> cookies, Encoding encoding)
        {
            return downloader.DownloadString(url, cookies, encoding);
        }

        public static void DownloadString(string url, string path)
        {
            downloader.DownloadString(url, path);
        }

        public static void DownloadString(string url, string path, Encoding encoding)
        {
            downloader.DownloadString(url, path, encoding);
        }

        public static byte[] DownloadData(string url)
        {
            return downloader.DownloadData(url);
        }

        public static byte[] DownloadData(string url, CancellationToken cancelToken)
        {
            return downloader.DownloadData(url, cancelToken);
        }

        public static void DownloadFile(string url, string path)
        {
            downloader.DownloadFile(url, path);
        }

        public static void DownloadFile(string url, string path, CancellationToken cancelToken)
        {
            downloader.DownloadFile(url, path, cancelToken);
        }

        public static HttpStatusCode GetResponseCode(string url, CancellationToken cancelToken, out Dictionary<string, string> headers)
        {
            headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "HEAD";
                request.Timeout = 15 * 1000;
                request.ReadWriteTimeout = 15 * 1000;
                using (cancelToken.Register(() => request.Abort()))
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    foreach (string key in response.Headers.AllKeys)
                    {
                        headers[key] = response.Headers[key];
                    }

                    return response.StatusCode;
                }
            }
            catch (WebException ex) when (ex.Response is HttpWebResponse errResponse)
            {
                try
                {
                    foreach (string key in errResponse.Headers.AllKeys)
                    {
                        headers[key] = errResponse.Headers[key];
                    }
                }
                catch
                {
                }

                return errResponse.StatusCode;
            }
            catch (Exception e)
            {
                logger.Error(e, $"Failed to get HTTP response for {url}.");
                return HttpStatusCode.ServiceUnavailable;
            }
        }
    }
}
