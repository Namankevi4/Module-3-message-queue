using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Infrastructure;
using Microsoft.Extensions.Options;
using ModelObjects;
using Services;

namespace ProcessingFileService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IOptions<ProcessingServiceOptions> _options;
        private readonly IFileReaderService _fileReaderService;
        private readonly IMessageQueueProvider _messageQueueProvider;
        private ConcurrentDictionary<string, int> _fileAlreadyInDownloadState = new ConcurrentDictionary<string, int>();

        public Worker(ILogger<Worker> logger, IOptions<ProcessingServiceOptions> options, IFileReaderService fileReaderService, IMessageQueueProvider messageQueueProvider)
        {
            _logger = logger;
            _options = options;
            _fileReaderService = fileReaderService;
            _messageQueueProvider = messageQueueProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Task MessageHandler(BinaryData body)
            {
                var fileModel = body.ToObjectFromJson<FilePortionModel>();

                if (_fileAlreadyInDownloadState.TryAdd(fileModel.FileName, 0))
                {
                    Console.WriteLine($"Download file {fileModel.FileName} started");
                }

                _fileReaderService.WriteFileByPortion(_options.Value.BufferSize, _options.Value.OutputFolderPath,
                    fileModel);
                _fileAlreadyInDownloadState[fileModel.FileName]++;

                if (_fileAlreadyInDownloadState[fileModel.FileName] == (fileModel.FileSize / _options.Value.BufferSize) + 1)
                {
                    Console.WriteLine($"Download file {fileModel.FileName} completed");
                }
                return Task.CompletedTask;
            }

            Task ErrorHandler(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Task.CompletedTask;
            }

            var client = _messageQueueProvider.CreateClient(_options.Value.QueueName, _options.Value.MessageQueueConnectionString);

            while (!stoppingToken.IsCancellationRequested)
            {
                await client.CreateProcessorMessage(MessageHandler, ErrorHandler);

                await Task.Delay(10000, stoppingToken);
            }

            await client.DisposeAsync();

        }
    }
}
