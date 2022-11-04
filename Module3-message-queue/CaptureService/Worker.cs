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
        private readonly IMessageQueueProvider _messageQueueProvider;

        private readonly List<string> _fileAlreadyProcessed = new List<string>();

        public Worker(ILogger<Worker> logger, IOptions<CaptureServiceOptions> options, IFileReaderService fileReaderService, IMessageQueueProvider messageQueueProvider)
        {
            _logger = logger;
            _options = options;
            _fileReaderService = fileReaderService;
            _messageQueueProvider = messageQueueProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var folderPath = this._options.Value.MonitoringFolderPath;
                var filesInFolder = Directory.EnumerateFiles(folderPath).Select(fileName => Path.Combine(folderPath, fileName)).ToList();
                var newFiles = filesInFolder.Except(_fileAlreadyProcessed).ToList();

                _fileAlreadyProcessed.AddRange(newFiles);

                await using (var client = _messageQueueProvider.CreateClient(_options.Value.QueueName, _options.Value.MessageQueueConnectionString))
                {
                    foreach (var newFile in newFiles)
                    {
                        async Task PortionHandler(FilePortionModel filePortion)
                        {
                            await client.SendMessage(filePortion);
                        }
                        Console.WriteLine($"Upload file '{newFile}' started");
                        await _fileReaderService.ReadFileByPortionAsync(_options.Value.BufferSize, newFile, PortionHandler);
                        Console.WriteLine($"Upload file '{newFile}' completed");

                        _fileAlreadyProcessed.Remove(newFile);

                    }
                }

                await Task.Delay(10000, stoppingToken);
            }
        }
    }
}
