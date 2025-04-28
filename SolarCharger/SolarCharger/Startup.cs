using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using SolarCharger.Controllers;
using SolarCharger.EF;
using SolarCharger.Services;
using System.Text.Json.Serialization;

namespace SolarCharger
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews()
                .AddJsonOptions(opt => { opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });
            services.AddScoped<SettingsService>();
            services.AddScoped<ChargeSessionService>();
            services.AddScoped<IPowerMeter, HomeWizardPowerMeter>();
            services.AddScoped<ITesla, Tesla>();
            //services.AddScoped<ITesla, TeslaSimulator>();
            services.AddScoped<IHubService, HubService>();
            services.AddSingleton<IStateEngine, StateEngine>();
            services.AddHttpClient<Tesla>()
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    return new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                    };
                });
            services.AddDbContext<ChargeContext, ChargeContext>();

            services.AddCors(options =>
            {
                options.AddPolicy("DevelopmentPolicy", policyBuilder =>
                {
                    policyBuilder.AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                        .SetIsOriginAllowed(hosts => true);
                });
            });

            services.AddSwaggerGen(setup =>
            {
                setup.SwaggerDoc("core", new OpenApiInfo
                {
                    Title = "SolarCharger",
                    Version = "v1"
                });
            });

            services.AddSignalR();
        }

        public void Configure(ILogger<Startup> logger, IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors("DevelopmentPolicy");
            app.UseDeveloperExceptionPage();
            app.UseSwagger(c =>
            {
                c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                {
                    swaggerDoc.Servers = new List<OpenApiServer>
                    {
                        new OpenApiServer
                        {
                            Url = $"{httpReq.Scheme}://{httpReq.Host.Value}"
                        }
                    };
                });
            });
            app.UseSwaggerUI(o => { o.SwaggerEndpoint("core/swagger.json", "SolarCharger"); });

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<SolarHub>("/solar_hub");
            });
        }
    }
}
