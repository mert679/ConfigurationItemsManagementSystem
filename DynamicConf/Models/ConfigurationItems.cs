using System.ComponentModel.DataAnnotations;

namespace DynamicConf.Models
{
    public class ConfigurationItems
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public string Value { get; set; }

        public bool IsActive { get; set; }

        [Required]
        public string ApplicationName { get; set; }
    }
}
