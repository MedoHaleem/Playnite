using Microsoft.Win32;
using Newtonsoft.Json;
using Playnite.Common;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Services
{
    public class ServicesClient : BaseServicesClient
    {
        public class RecommendedAddons
        {
            public Dictionary<string, string> Libraries { get; set; }
            public Dictionary<string, string> Generic { get; set; }
        }

        private static ILogger logger = LogManager.GetLogger();

        public ServicesClient() : this(ConfigurationManager.AppSettings["ServicesUrl"])
        {
        }

        public ServicesClient(string endpoint) : base(endpoint, Updater.CurrentVersion)
        {
        }

        public List<string> GetPatrons()
        {
            return ExecuteGetRequest<List<string>>("/patreon/patrons");
        }

        public Guid UploadDiagPackage(string diagPath)
        {
            var url = Endpoint + "/playnite/diag";
            var request = CreateRequest(url, "POST");
            request.ContentType = "application/octet-stream";

            try
            {
                using (var fs = new FileStream(diagPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    request.ContentLength = fs.Length;
                    using (var reqStream = request.GetRequestStream())
                    {
                        fs.CopyTo(reqStream);
                    }
                }

                using (var response = (HttpWebResponse)request.GetResponse())
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    var strResult = reader.ReadToEnd();
                    var result = JsonConvert.DeserializeObject<ServicesResponse<Guid>>(strResult);
                    if (!string.IsNullOrEmpty(result.Error))
                    {
                        logger.Error("Service request error by proxy: " + result.Error);
                        throw new Exception(result.Error);
                    }

                    return result.Data;
                }
            }
            catch (WebException ex) when (ex.Response is HttpWebResponse errorResponse)
            {
                using (errorResponse)
                using (var stream = errorResponse.GetResponseStream())
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    var strResult = reader.ReadToEnd();
                    var result = JsonConvert.DeserializeObject<ServicesResponse<Guid>>(strResult);
                    if (!string.IsNullOrEmpty(result.Error))
                    {
                        logger.Error("Service request error by proxy: " + result.Error);
                        throw new Exception(result.Error);
                    }

                    return result.Data;
                }
            }
        }

        public List<AddonManifest> GetAllAddons(AddonType type, string searchTerm)
        {
            return ExecuteGetRequest<List<AddonManifest>>($"/addons?type={type}&searchTerm={searchTerm}".UrlEncode());
        }

        public AddonManifest GetAddon(string addonId)
        {
            return ExecuteGetRequest<List<AddonManifest>>($"/addons?addonId={addonId}".UrlEncode()).FirstOrDefault();
        }

        public AddonInstallerManifest GetAddonInstaller(string addonId)
        {
            return ExecuteGetRequest<AddonInstallerManifest>($"/addons/installer?addonId={addonId}".UrlEncode());
        }

        public string[] GetAddonBlacklist()
        {
            return ExecuteGetRequest<string[]>("/addons/blacklist") ?? new string[0];
        }

        public RecommendedAddons GetDefaultExtensions()
        {
            var stringData = ExecuteGetRequest<string>("/addons/defaultextensions");
            if (stringData.IsNullOrEmpty())
            {
                return new RecommendedAddons();
            }
            else
            {
                return Serialization.FromJson<RecommendedAddons>(stringData);
            }
        }
    }
}
