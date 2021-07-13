using System.Net.Http;
using System.Threading.Tasks;
using PluginHubspot.Helper;

namespace PluginHubspot.API.Factory
{
    public interface IApiClient
    {
        Task TestConnection();
        Task<HttpResponseMessage> GetAsync(string path, bool is_full_path = false);
        Settings GetSettings();
        // Task<HttpResponseMessage> PostAsync(string path, StringContent json);
        Task<HttpResponseMessage> PostAsync(string path, string json);
        // Task<HttpResponseMessage> PutAsync(string path, StringContent json);
        Task<HttpResponseMessage> PutAsync(string path, string json);
        // Task<HttpResponseMessage> PatchAsync(string path, StringContent json);
        Task<HttpResponseMessage> PatchAsync(string path, string json);
        Task<HttpResponseMessage> DeleteAsync(string path);
    }
}