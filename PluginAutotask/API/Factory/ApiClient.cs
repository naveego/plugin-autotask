using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Naveego.Sdk.Logging;
using Newtonsoft.Json;
using PluginHubspot.API.Utility;
using PluginHubspot.Helper;

namespace PluginHubspot.API.Factory
{
    public class ApiClient: IApiClient
    {
       // private IApiAuthenticator Authenticator { get; set; }
        private static HttpClient Client { get; set; }
        private Settings Settings { get; set; }

        //private const string ApiKeyParam = "hapikey";

        public ApiClient(HttpClient client, Settings settings)
        {
            //Authenticator = new ApiAuthenticator(client, settings);
            Client = client;
            Settings = settings;
            
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public Settings GetSettings()
        {
            return Settings;
        }
        
        public async Task TestConnection()
        {
            try
            {
                //var token = await Authenticator.GetToken();
                var uriBuilder = new UriBuilder(
                    $"{Constants.HttpsPrefix.TrimEnd('.')}" +
                    $"{Settings.ApiZone.TrimEnd('.')}" +
                    $"." +
                    $"{Constants.DomainApiUrl.TrimEnd('/')}" +
                    $"/" +
                    $"{Constants.TestConnectionPath.TrimStart('/')}"
                    );
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);

                uriBuilder.Query = query.ToString();
                
                var uri = new Uri(uriBuilder.ToString());
                
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = uri,
                };
                {
                    request.Headers.Add("Username", Settings.UserName);
                    request.Headers.Add("Secret", Settings.Secret);
                    request.Headers.Add("ApiIntegrationCode", Settings.ApiIntegrationCode);
                }

                
                var response = await Client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(await response.Content.ReadAsStringAsync());
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }
        }

        public async Task<HttpResponseMessage> GetAsync(string path, bool is_full_path = false)
        {
            try
            {
                var uriBuilder = new UriBuilder(
                    is_full_path ? 
                    path //path is full path, do not build
                    :    //else, build path
                    $"{Constants.HttpsPrefix}" +
                    $"{Settings.ApiZone.TrimEnd('.')}" +
                    $"." +
                    $"{Constants.DomainApiUrl.TrimEnd('/')}" +
                    $"/" +
                    $"{path.TrimStart('/')}"
                    );
                
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);

                uriBuilder.Query = query.ToString();
                
                var uri = new Uri(uriBuilder.ToString());
                
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = uri,
                };

                
                request.Headers.Add("Username", Settings.UserName);
                request.Headers.Add("Secret", Settings.Secret);
                request.Headers.Add("ApiIntegrationCode", Settings.ApiIntegrationCode);
                
                return await Client.SendAsync(request);
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }
        }

        // public async Task<HttpResponseMessage> PostAsync(string path, StringContent json)
        public async Task<HttpResponseMessage> PostAsync(string path, string json)
        {
            try
            {
                //var token = await Authenticator.GetToken();
                var uriBuilder = new UriBuilder($"{Constants.DomainApiUrl.TrimEnd('/')}/{path.TrimStart('/')}");
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);

                uriBuilder.Query = query.ToString();
                
                var uri = new Uri(uriBuilder.ToString());
                
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = uri,
                    //Content = json
                };
                var values =
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                foreach (var value in values)
                {
                    request.Headers.Add(value.Key, value.Value);
                }
                
                return await Client.SendAsync(request);
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }
        }

        // public async Task<HttpResponseMessage> PutAsync(string path, StringContent json)
        public async Task<HttpResponseMessage> PutAsync(string path, string json)
        {
            try
            {
                //var token = await Authenticator.GetToken();
                var uriBuilder = new UriBuilder($"{Constants.DomainApiUrl.TrimEnd('/')}/{path.TrimStart('/')}");
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);

                uriBuilder.Query = query.ToString();
                
                var uri = new Uri(uriBuilder.ToString());
                
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Put,
                    RequestUri = uri,
                    //Content = json
                };
                var values =
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                foreach (var value in values)
                {
                    request.Headers.Add(value.Key, value.Value);
                }

                return await Client.SendAsync(request);
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw;
            }
        }

          public async Task<HttpResponseMessage> PatchAsync(string path, string json)
        {
            try
            {
                var uriBuilder = new UriBuilder($"{Constants.HttpsPrefix}" +
                                                $"{Settings.ApiZone.TrimEnd('.')}" +
                                                $"." +
                                                $"{Constants.DomainApiUrl.TrimEnd('/')}" +
                                                $"/" +
                                                $"{path.TrimStart('/')}"
                );
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);

                uriBuilder.Query = query.ToString();
                
                var uri = new Uri(uriBuilder.ToString());


                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Patch,
                    RequestUri = uri,
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
                
                request.Headers.Add("Username", Settings.UserName);
                request.Headers.Add("Secret", Settings.Secret);
                request.Headers.Add("ApiIntegrationCode", Settings.ApiIntegrationCode);
                
                
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
                //var token = await Authenticator.GetToken();
                var uriBuilder = new UriBuilder($"{Constants.DomainApiUrl.TrimEnd('/')}/{path.TrimStart('/')}");
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