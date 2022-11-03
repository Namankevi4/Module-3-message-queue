using System;
using System.Collections.Generic;
using System.Text;

namespace ModelObjects
{
    public class ProcessingServiceOptions
    {
        public const string ProcessingServiceConfiguration = "ProcessingServiceConfiguration";

        public string MessageQueueConnectionString { get; set; } = String.Empty;
        public string QueueName { get; set; } = String.Empty;
        public string OutputFolderPath { get; set; } = String.Empty;
        public int BufferSize { get; set; } = 64000;

    }
}
