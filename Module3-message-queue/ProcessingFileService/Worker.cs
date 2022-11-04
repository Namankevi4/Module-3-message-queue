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
        private readonly ConcurrentDictionary<string, int> _fileAlreadyInDownloadState = new ConcurrentDictionary<string, int>();
        private readonly MessageQueueClient _client;

        public Worker(ILogger<Worker> logger, IOptions<ProcessingServiceOptions> options, IFileReaderService fileReaderService, MessageQueueClient client)
        {
            this._client = client;
            this._client.RegisterMesageHandlers(MessageHandler, ErrorHandler).GetAwaiter().GetResult();
            this._client.StartProcessingAsync().GetAwaiter().GetResult();

            _logger = logger;
            _options = options;
            _fileReaderService = fileReaderService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(10000, stoppingToken);
            }
        }

        private Task MessageHandler(BinaryData body)
        {
            var fileModel = body.ToObjectFromJson<FilePortionModel>();

            if (_fileAlreadyInDownloadState.TryAdd(fileModel.FileName, 0))
            {
                _logger.LogInformation($"Download file {fileModel.FileName} started");
            }

            _fileReaderService.WriteFileByPortion(fileModel.BufferSize, _options.Value.OutputFolderPath,
                fileModel);
            _fileAlreadyInDownloadState[fileModel.FileName]++;

            if (_fileAlreadyInDownloadState[fileModel.FileName] == Math.Ceiling((decimal)(fileModel.FileSize / fileModel.BufferSize)))
            {
                _logger.LogInformation($"Download file {fileModel.FileName} completed");
            }
            return Task.CompletedTask;
        }

        private Task ErrorHandler(Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Task.CompletedTask;
        }
    }
}
