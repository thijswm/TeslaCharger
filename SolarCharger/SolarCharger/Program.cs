using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SolarCharger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SolarCharger.EF;

var currentAssembly = Assembly.GetExecutingAssembly();
var fullPath = currentAssembly.Location;
var currentAssemblyPath = Path.GetDirectoryName(fullPath);

var configuration = new ConfigurationBuilder()
    .SetBasePath(currentAssemblyPath!)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

using var host = Host.CreateDefaultBuilder(args)
    .UseSerilog((hostingContext, services, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(configuration))
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.UseConfiguration(configuration);
        webBuilder.UseStartup<Startup>();
    })
    .UseWindowsService()
    .Build();

var logger = host.Services.GetRequiredService<ILoggerFactory>();
var log = logger.CreateLogger<Startup>();
log.LogInformation("Starting Solar Charger");

try
{
    log.LogInformation("Migrating database");
    var dbContext = host.Services.GetRequiredService<ChargeContext>();
    dbContext.Database.Migrate();
    log.LogInformation("Database Migrated");
}
catch (Exception ex)
{
    log.LogError(ex, "Error migrating database");
    return;
}

await host.RunAsync();