using System;

namespace ModelObjects
{
    public class CaptureServiceOptions
    {
        public const string CaptureServiceConfiguration = "CaptureServiceConfiguration";

        public string MessageQueueConnectionString { get; set; } = String.Empty;
        public string QueueName { get; set; } = String.Empty;
        public string MonitoringFolderPath { get; set; } = String.Empty;
        public int BufferSize { get; set; } = 64000;
    }
}
