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
            _processor = _client.CreateProcessor(this._queueName, new ServiceBusProcessorOptions());
        }

        public async Task RegisterMesageHandlers(Func<BinaryData, Task> messageHandler,
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

            _processor.ProcessMessageAsync += MessageHandler;
            _processor.ProcessErrorAsync += ErrorHandler;
        }

        public async Task StartProcessingAsync()
        {
            await _processor.StartProcessingAsync();
        }

        public async Task StopProcessingAsync()
        {
            await _processor.StopProcessingAsync();
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
            await _processor.DisposeAsync();
        }
    }
}
