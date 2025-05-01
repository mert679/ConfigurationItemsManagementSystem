using Dapper;
using Npgsql;
namespace DynamicConf.ConfigurationLibrary
{
    public class ConfigurationReader
    {
        private readonly string _appName;
        private readonly string _connectionString;
        private readonly System.Timers.Timer _timer;
        private readonly Dictionary<string, ConfigurationItems> _cache = new();
        private readonly int _refreshIntervalMs;

        public ConfigurationReader(string applicationName, string connectionString, int refreshIntervalMs)
        {
            _appName = applicationName;
            _connectionString = connectionString;
            _refreshIntervalMs = refreshIntervalMs;

            Console.WriteLine($"[Init] ConfigurationReader initialized for App: '{_appName}' with refresh interval: {_refreshIntervalMs}ms");
            
            LoadConfigurations();
            _timer = new System.Timers.Timer(refreshIntervalMs);
            _timer.Elapsed += (sender, args) => LoadConfigurations();
            _timer.Start();

        }

        private void LoadConfigurations()
        {
            Console.WriteLine("[Load] Starting to load configurations...");
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                Console.WriteLine("[DB] Attempting to open database connection...");
                connection.Open();
                Console.WriteLine("[DB] Database connection opened successfully.");

                string sqlQuery = "SELECT * FROM \"ConfigurationItems\" WHERE \"IsActive\" = TRUE AND \"ApplicationName\" = @App";

                Console.WriteLine($"[SQL] Executing query: {sqlQuery}");

                var result = connection.Query<ConfigurationItems>(sqlQuery, new { App = _appName }).ToList();
                Console.WriteLine($"[SQL] Query returned {result.Count} record(s).");
                lock (_cache)
                {
                    _cache.Clear();
                    foreach (var item in result)
                    {
                        Console.WriteLine($"[Cache] Caching item: {item.Name} = {item.Value}");
                        _cache[item.Name] = item;
                    }
                }
                Console.WriteLine("[Load] Configuration cache updated successfully.");  
            }
            catch (Exception ex)
            {
                // Handle exception
                Console.WriteLine($"Error loading configurations: {ex.Message}");
                Console.WriteLine(ex.ToString());
            }
        }
        public T GetValue<T>(string key)
        {
            lock (_cache)
            {
                if (_cache.TryGetValue(key, out var configItem))
                {
                    try
                    {
                        Console.WriteLine($"[GetValue] Retrieved value for key '{key}': {configItem.Value}");
                        return (T)Convert.ChangeType(configItem.Value, typeof(T));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[GetValue] Conversion error for key '{key}': {ex.Message}");
                        throw new InvalidCastException($"Cannot convert value '{configItem.Value}' to type '{typeof(T).Name}'", ex);
                    }
                }
                else
                {
                    Console.WriteLine($"[GetValue] Key not found in cache: '{key}'");
                    throw new KeyNotFoundException($"Configuration item '{key}' not found.");
                }
            }
        }
    }
}
