using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SolarCharger.EF
{
    public class ChargeContext : DbContext
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ChargeContext> _log;
        public virtual DbSet<Settings> Settings { get; set; }
        public virtual DbSet<ChargeSession> ChargeSessions { get; set; }
        public virtual DbSet<ChargeCurrentChange> ChargeCurrentChanges { get; set; }

        public ChargeContext(IConfiguration configuration, ILogger<ChargeContext> log)
        {
            _configuration = configuration;
            _log = log;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var configFolder = Environment.GetEnvironmentVariable("ConfigFolder") ?? "";
            if (string.IsNullOrEmpty(configFolder))
            {
                var currentAssembly = Assembly.GetExecutingAssembly();
                configFolder = Path.GetDirectoryName(currentAssembly.Location);
            }
            var sqliteFile = Path.Combine(configFolder, "solar_charger.db");

            _log.LogInformation($"Using SQLite file: {sqliteFile}");
            var connectionString = $"Data Source={sqliteFile}";
            optionsBuilder.UseSqlite(connectionString);
        }
    }
}
