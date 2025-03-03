using MudBlazor.Services;
using SolarChargerFE.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace SolarChargerFE
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var apiAddress = Environment.GetEnvironmentVariable("ApiAddress") ?? "http://server:8080/";

            Uri? hubAddress = null;
            if (Uri.TryCreate(apiAddress, UriKind.Absolute, out var uri))
            {
                hubAddress = new Uri(uri, "solar_hub");
            }

            if (hubAddress == null)
            {
                Console.WriteLine($"Invalid api address given: '{apiAddress}'");
                Environment.Exit(1);
                return;
            }

            Console.WriteLine($"Using api address: '{apiAddress}' and hub_address: '{hubAddress}'");

            builder.Services.AddMudServices();

            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddHttpClient<SolarChargerClient>(client =>
            {
                client.BaseAddress = new Uri(apiAddress);
            });

            builder.Services.AddSingleton(provider =>
            {
                var hubConnection = new HubConnectionBuilder()
                    .WithUrl(hubAddress!)
                    .WithAutomaticReconnect()
                    .Build();
                return hubConnection;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
