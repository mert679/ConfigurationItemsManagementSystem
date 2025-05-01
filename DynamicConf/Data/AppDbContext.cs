using DynamicConf.Models;
using Microsoft.EntityFrameworkCore;

namespace DynamicConf.Data
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options):base(options) { }
        public DbSet<ConfigurationItems> ConfigurationItems { get; set; }
    }
}
