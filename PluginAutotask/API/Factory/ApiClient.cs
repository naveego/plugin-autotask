using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Aunalytics.Sdk.Logging;
using Newtonsoft.Json;
using PluginAutotask.API.Utility;
using PluginAutotask.Helper;

namespace PluginAutotask.API.Factory
{
    public class ApiClient: IApiClient
    {
        private static HttpClient Client { get; set; }
        private Settings Settings { get; set; }
        private string BaseApiUrl { get; set; }

        public ApiClient(HttpClient client, Settings settings)
        {
            Client = client;
            Settings = settings;
            BaseApiUrl = $"https://{Settings.ApiZone}.autotask.net/ATServicesRest/V1.0/";

            Client.DefaultRequestHeaders.Clear();
            
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            Client.DefaultRequestHeaders.Add("Username", Settings.UserName);
            Client.DefaultRequestHeaders.Add("Secret", Settings.Secret);
            Client.DefaultRequestHeaders.Add("ApiIntegrationCode", Settings.ApiIntegrationCode);
        }

        public Settings GetSettings()
        {
            return Settings;
        }
        
        public async Task TestConnection()
        {
            try
            {
                var response = await GetAsync(Constants.TestConnectionPath);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }
        }

        public async Task<HttpResponseMessage> GetAsync(string path)
        {
            try
            {
                var uriBuilder = new UriBuilder($"{BaseApiUrl.TrimEnd('/')}/{path.Replace(BaseApiUrl, "").TrimStart('/')}");
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                uriBuilder.Query = query.ToString();
                
                var uri = new Uri(uriBuilder.ToString());
                
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = uri,
                };

                return await Client.SendAsync(request);
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }
        }

        public async Task<HttpResponseMessage> PostAsync(string path, StringContent json)
        {
            try
            {
                var uriBuilder = new UriBuilder($"{BaseApiUrl.TrimEnd('/')}/{path.Replace(BaseApiUrl, "").TrimStart('/')}");
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                uriBuilder.Query = query.ToString();
                
                var uri = new Uri(uriBuilder.ToString());
                
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = uri,
                    Content = json
                };

                return await Client.SendAsync(request);
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }
        }

        public async Task<HttpResponseMessage> PutAsync(string path, StringContent json)
        {
            try
            {
                var uriBuilder = new UriBuilder($"{BaseApiUrl.TrimEnd('/')}/{path.Replace(BaseApiUrl, "").TrimStart('/')}");
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                uriBuilder.Query = query.ToString();
                
                var uri = new Uri(uriBuilder.ToString());
                
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Put,
                    RequestUri = uri,
                    Content = json
                };

                return await Client.SendAsync(request);
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }
        }

        public async Task<HttpResponseMessage> PatchAsync(string path, StringContent json)
        {
            try
            {
                var uriBuilder = new UriBuilder($"{BaseApiUrl.TrimEnd('/')}/{path.Replace(BaseApiUrl, "").TrimStart('/')}");
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                uriBuilder.Query = query.ToString();
                
                var uri = new Uri(uriBuilder.ToString());
                
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Patch,
                    RequestUri = uri,
                    Content = json
                };

                return await Client.SendAsync(request);
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }
        }

        public async Task<HttpResponseMessage> DeleteAsync(string path)
        {
            try
            {
                var uriBuilder = new UriBuilder($"{BaseApiUrl.TrimEnd('/')}/{path.Replace(BaseApiUrl, "").TrimStart('/')}");
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                uriBuilder.Query = query.ToString();
                
                var uri = new Uri(uriBuilder.ToString());
                
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Delete,
                    RequestUri = uri
                };

                return await Client.SendAsync(request);
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }
        }
    }
}