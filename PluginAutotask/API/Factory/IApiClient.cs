using System.Net.Http;
using System.Threading.Tasks;
using PluginAutotask.Helper;

namespace PluginAutotask.API.Factory
{
    public interface IApiClient
    {
        Task TestConnection();
        Settings GetSettings();
        Task<HttpResponseMessage> GetAsync(string path);
        Task<HttpResponseMessage> PostAsync(string path, StringContent json);
        Task<HttpResponseMessage> PutAsync(string path, StringContent json);
        Task<HttpResponseMessage> PatchAsync(string path, StringContent json);
        Task<HttpResponseMessage> DeleteAsync(string path);
    }
}

