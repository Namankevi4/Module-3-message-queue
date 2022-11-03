using System;
using System.Collections.Generic;
using System.Text;
using Azure.Messaging.ServiceBus;

namespace ProcessingService
{
    public class ServiceBusProvider
    {
        // the client that owns the connection and can be used to create senders and receivers
        private readonly ServiceBusClient client;

        // the processor that reads and processes messages from the queue
        private readonly ServiceBusProcessor processor;

        public ServiceBusProvider()
        {
            var clientOptions = new ServiceBusClientOptions()
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets
            };
            client = new ServiceBusClient("Endpoint=sb://mentoringalexn.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=f/U9NkQmZAdF3GHh/0UoP0XMnmqcKjts5n5rRHJ2QN4=", clientOptions);

            processor = client.CreateProcessor("documentqueue", new ServiceBusProcessorOptions());

            try
            {
                // add handler to process messages
                processor.ProcessMessageAsync += MessageHandler;

                // add handler to process any errors
                processor.ProcessErrorAsync += ErrorHandler;

                // start processing 
                await processor.StartProcessingAsync();

                Console.WriteLine("Wait for a minute and then press any key to end the processing");
                Console.ReadKey();

                // stop processing 
                Console.WriteLine("\nStopping the receiver...");
                await processor.StopProcessingAsync();
                Console.WriteLine("Stopped receiving messages");
            }
            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                await processor.DisposeAsync();
                await client.DisposeAsync();
            }
        }
    }
}
