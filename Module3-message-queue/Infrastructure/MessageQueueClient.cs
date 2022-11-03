using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using ModelObjects;

namespace Infrastructure
{
    public class MessageQueueClient: IAsyncDisposable
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _sender;
        private ServiceBusProcessor _processor;
        private readonly string _queueName;

        public MessageQueueClient(string queueName, string connStr)
        {
            this._queueName = queueName;
            var clientOptions = new ServiceBusClientOptions()
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets
            };

            _client = new ServiceBusClient(connStr, clientOptions);
            _sender = _client.CreateSender(queueName);
        }

        public async Task CreateProcessorMessage(Func<BinaryData, Task> messageHandler,
            Func<Exception, Task> errorHandler)
        {
            async Task MessageHandler(ProcessMessageEventArgs args)
            {
                var body = args.Message.Body;

                await messageHandler(body);

                await args.CompleteMessageAsync(args.Message);
            }

            async Task ErrorHandler(ProcessErrorEventArgs args)
            {
                await errorHandler(args.Exception);
            }

            _processor = _client.CreateProcessor(this._queueName, new ServiceBusProcessorOptions());
            _processor.ProcessMessageAsync += MessageHandler;
            _processor.ProcessErrorAsync += ErrorHandler;

            await _processor.StartProcessingAsync();
        }
        
        public async Task SendMessage(FilePortionModel message)
        {
            BinaryData body = new BinaryData(message);
            ServiceBusMessage messageSb = new ServiceBusMessage(body);
            await _sender.SendMessageAsync(messageSb);
        }

        public async ValueTask DisposeAsync()
        {
            await _sender.DisposeAsync();
            await _client.DisposeAsync();
            if (_processor != null)
            {
                await _processor.DisposeAsync();
            }
        }
    }
}
