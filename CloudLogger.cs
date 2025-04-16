using Google.Cloud.Logging.V2;
using Google.Api;
using Google.Protobuf.WellKnownTypes;
using Google.Cloud.Logging.Type;

namespace TextToSpeechApiDemo
{
    // Logging helper class
    public static class CloudLogger
    {
        private static readonly LoggingServiceV2Client? _loggingClient;
        private static readonly string? _logName;
        private static readonly string? _projectId;

        static CloudLogger()
        {
            // Initialize the Logging client.
            _projectId = Environment.GetEnvironmentVariable("GOOGLE_CLOUD_PROJECT");
            if (string.IsNullOrEmpty(_projectId))
            {
                Console.WriteLine("Warning: GOOGLE_CLOUD_PROJECT environment variable is not set. Cloud Logging will be disabled.");
                return;
            }

            try
            {
                _loggingClient = LoggingServiceV2Client.Create();
                _logName = $"projects/{_projectId}/logs/text-to-speech-api-demo";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing Cloud Logging client: {ex.Message}");
            }
        }

        public static void Log(string message, LogSeverity severity = LogSeverity.Info, Dictionary<string, string>? labels = null)
        {
            if (_loggingClient == null)
            {
                return; // Cloud Logging is not configured.
            }

            try
            {
                var logEntry = new LogEntry
                {
                    LogName = _logName,
                    Severity = severity,
                    Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
                    TextPayload = message,
                };

                if (labels != null)
                {
                    logEntry.Labels.Add(labels);
                }

                var request = new WriteLogEntriesRequest
                {
                    LogName = _logName,
                    Resource = new MonitoredResource { Type = "global" },
                };
                request.Entries.Add(logEntry);

                _loggingClient.WriteLogEntries(request);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to Cloud Logging: {ex.Message}");
            }
        }
    }
}
