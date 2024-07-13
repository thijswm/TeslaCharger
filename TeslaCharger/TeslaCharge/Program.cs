using MudBlazor.Services;
using TeslaCharge.Components;
using Serilog;
using TeslaCharge.HostedService;
using TeslaCharge.Services;

namespace TeslaCharge
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
            {
                loggerConfiguration
                    .ReadFrom.Configuration(hostingContext.Configuration);
            });

            builder.Services.AddMudServices();

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddInteractiveWebAssemblyComponents();

            builder.Services.Configure<Settings>(builder.Configuration.GetSection("Settings"));
            builder.Services.AddSingleton<IPowerMeter, HomeWizardPowerMeter>();
            builder.Services.AddSingleton<IPowerService, PowerService>();
            builder.Services.AddSingleton<ITeslaService, TeslaService>();
            builder.Services.AddSingleton<ITeslaCharger, TeslaCharger>();

            builder.Services.AddHostedService<PowerHostedService>();
            builder.Services.AddHostedService<TeslaMateHostedService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .AddInteractiveWebAssemblyRenderMode();

            app.Run();
        }
    }
}
