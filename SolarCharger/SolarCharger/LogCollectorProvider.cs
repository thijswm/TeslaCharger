using Serilog.Core;
using Serilog.Events;
using System.Collections.Concurrent;
using Serilog;
using Serilog.Configuration;

namespace SolarCharger
{
    public class LogCollectorProvider : ILogEventSink
    {
        public static readonly ConcurrentQueue<string> LogQueue = new();

        public void Emit(LogEvent logEvent)
        {
            var rendered = logEvent.RenderMessage();
            LogQueue.Enqueue($"{logEvent.Timestamp:HH:mm:ss} [{logEvent.Level}] {rendered}");
        }
    }

    public static class LogCollectorExtensions
    {
        public static LoggerConfiguration CollectLogs(this LoggerSinkConfiguration loggerConfiguration)
        {
            return loggerConfiguration.Sink(new LogCollectorProvider());
        }
    }

}
