using DynamicConf.Data;
using DynamicConf.Interface;
using DynamicConf.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Mvc;

namespace DynamicConf.Repository
{
    public class ConfigurationRepository : IConfigurationRepository
    {
        private readonly AppDbContext _context;

        public ConfigurationRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<ConfigurationItems>> GetAllConfigurations(string? search)
        {
            if (!string.IsNullOrEmpty(search))
            {
                var GetAll = await _context.ConfigurationItems
                    .Where(x => x.Name.Contains(search) || x.ApplicationName.Contains(search))
                    .ToListAsync();
                return GetAll;
            }
            else
            {
                var GetAll = await _context.ConfigurationItems.ToListAsync();
                return GetAll;
            }
          
        }
        public Task<ConfigurationItems> GetConfigurationById(int id)
        {
            var GetById = _context.ConfigurationItems.FirstOrDefaultAsync(x => x.Id == id);
            if (GetById == null)
            {
                throw new Exception("Configuration not found");
            }
            return GetById;
        }
      
        public async Task<ConfigurationItems> AddConfiguration(ConfigurationItems configuration)
        {
            
            _context.Add(configuration);
            await _context.SaveChangesAsync();
            return configuration;

        }
        public  async Task<ConfigurationItems> UpdateConfiguration(ConfigurationItems configuration)
        {
            _context.Update(configuration);
            await _context.SaveChangesAsync();
            return configuration;
        }


      
    }
    
}
