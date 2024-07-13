using System.Text;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using TeslaCharge.Services;

namespace TeslaCharge.HostedService
{
    public class TeslaMateHostedService : IHostedService
    {
        private readonly ILogger<TeslaMateHostedService> _log;
        private readonly Settings _settings;
        private readonly ITeslaService _teslaService;
        private IMqttClient? _mqttClient;
        private TeslaData _teslaData;

        public TeslaMateHostedService(ILogger<TeslaMateHostedService> log, IOptions<Settings> settings, ITeslaService teslaService)
        {
            _log = log;
            _settings = settings.Value;
            _teslaService = teslaService;
            _teslaData = new TeslaData();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(_settings.MqttTeslaMate))
            {
                throw new InvalidOperationException("MqttTeslaMate is not set.");
            }

            var mqttFactory = new MqttFactory();
            _mqttClient = mqttFactory.CreateMqttClient();

            _mqttClient.ConnectedAsync += _mqttClient_ConnectedAsync;
            _mqttClient.ApplicationMessageReceivedAsync += MqttClient_ApplicationMessageReceivedAsync;
            _mqttClient.DisconnectedAsync += _mqttClient_DisconnectedAsync;

            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithClientId("TeslaCharge")
                .WithTcpServer(_settings.MqttTeslaMate)
                .WithCleanSession()
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(60))
                .Build();

            await _mqttClient.ConnectAsync(mqttClientOptions, cancellationToken);

            var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder().WithTopicFilter(f =>
                {
                    f.WithTopic("teslamate/cars/1/battery_level");
                    f.WithAtLeastOnceQoS();
                })
                .WithTopicFilter(f =>
                {
                    f.WithTopic("teslamate/cars/1/state");
                    f.WithAtLeastOnceQoS();
                })
                .WithTopicFilter(f =>
                {
                    f.WithTopic("teslamate/cars/1/plugged_in");
                    f.WithAtLeastOnceQoS();
                })
                .WithTopicFilter(f =>
                {
                    f.WithTopic("teslamate/cars/1/charge_limit_soc");
                    f.WithAtLeastOnceQoS();
                })
                .Build();

            var response = await _mqttClient.SubscribeAsync(mqttSubscribeOptions, cancellationToken);

            response.Items.ToList().ForEach(item =>
            {
                _log.LogInformation($"Subscribed to {item.TopicFilter.Topic}");
            });
        }

        private Task _mqttClient_DisconnectedAsync(MqttClientDisconnectedEventArgs arg)
        {
            _log.LogInformation("Disconnected from TeslaMate MQTT");
            return Task.CompletedTask;
        }

        private Task _mqttClient_ConnectedAsync(MqttClientConnectedEventArgs arg)
        {
            _log.LogInformation("Connected to TeslaMate MQTT");
            return Task.CompletedTask;
        }

        private Task MqttClient_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
        {
            if (arg.ApplicationMessage.Topic == "teslamate/cars/1/battery_level")
            {
                try
                {
                    var batteryLevel = Encoding.UTF8.GetString(arg.ApplicationMessage.Payload);
                    _teslaData.BatteryLevel = int.Parse(batteryLevel);
                    _log.LogInformation($"Battery level: {batteryLevel}");
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Failed to parse battery level");
                }
            }
            else if (arg.ApplicationMessage.Topic == "teslamate/cars/1/state")
            {
                try
                {
                    var state = Encoding.UTF8.GetString(arg.ApplicationMessage.Payload);
                    _teslaData.State = state;
                    _log.LogInformation($"State: {state}");
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Failed to parse state");
                }
            }
            else if (arg.ApplicationMessage.Topic == "teslamate/cars/1/plugged_in")
            {
                try
                {
                    var pluggedIn = Encoding.UTF8.GetString(arg.ApplicationMessage.Payload);
                    _teslaData.PluggedIn = bool.Parse(pluggedIn);
                    _log.LogInformation($"Plugged in: {pluggedIn}");
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Failed to parse plugged in");
                }
            }
            else if (arg.ApplicationMessage.Topic == "teslamate/cars/1/charge_limit_soc")
            {
                try
                {
                    var chargeLimitSoc = Encoding.UTF8.GetString(arg.ApplicationMessage.Payload);
                    _teslaData.ChargeLimitSoc = int.Parse(chargeLimitSoc);
                    _log.LogInformation($"Charge limit SOC: {chargeLimitSoc}");
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Failed to parse charge limit SOC");
                }
            }

            if (_teslaService is TeslaService teslaService)
            {
                teslaService.UpdateData(_teslaData);
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
