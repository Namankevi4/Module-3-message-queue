using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.Extensions.Options;
using ModelObjects;
using Services;

namespace CaptureService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IOptions<CaptureServiceOptions> _options;
        private readonly IFileReaderService _fileReaderService;
        private readonly MessageQueueClient _client;

        private readonly List<string> _fileAlreadyProcessed = new List<string>();

        public Worker(ILogger<Worker> logger, IOptions<CaptureServiceOptions> options, IFileReaderService fileReaderService, MessageQueueClient client)
        {
            _client = client;
            _logger = logger;
            _options = options;
            _fileReaderService = fileReaderService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var folderPath = this._options.Value.MonitoringFolderPath;
                var filesInFolder = Directory.EnumerateFiles(folderPath)
                    .Select(fileName => Path.Combine(folderPath, fileName)).ToList();
                var newFiles = filesInFolder.Except(_fileAlreadyProcessed).ToList();

                _fileAlreadyProcessed.AddRange(newFiles);

                foreach (var newFile in newFiles)
                {
                    _logger.LogInformation($"Upload file '{newFile}' started");

                    foreach (var filePortion in _fileReaderService.ReadFileByPortion(_options.Value.BufferSize, newFile))
                    {
                        await _client.SendMessage(filePortion);
                    }

                    _logger.LogInformation($"Upload file '{newFile}' completed");
                }

                await Task.Delay(10000, stoppingToken);
            }
        }
    }
}
