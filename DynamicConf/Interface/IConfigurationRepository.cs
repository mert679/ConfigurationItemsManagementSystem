using DynamicConf.Models;

namespace DynamicConf.Interface
{
    public interface IConfigurationRepository
    {
        // we need to add list, add, update, delete methods
        Task<List<ConfigurationItems>> GetAllConfigurations(string? search);
        Task<ConfigurationItems> GetConfigurationById(int id);
        Task<ConfigurationItems> AddConfiguration(ConfigurationItems configuration);
        Task<ConfigurationItems> UpdateConfiguration(ConfigurationItems configuration);
    }
}
