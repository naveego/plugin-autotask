using PluginAutotask.Helper;

namespace PluginAutotask.API.Factory
{
    public interface IApiClientFactory
    {
        IApiClient CreateApiClient(Settings settings);
    }
}