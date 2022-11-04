using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.Extensions.Configuration;
using ModelObjects;
using Services;

namespace ProcessingFileService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<ProcessingServiceOptions>(
                        hostContext.Configuration.GetSection(ProcessingServiceOptions.ProcessingServiceConfiguration));
                    services.AddSingleton<IFileReaderService, FileReaderService>(); ;
                    services.AddHostedService<Worker>();

                    var configOptions = new ProcessingServiceOptions();
                    hostContext.Configuration.GetSection(ProcessingServiceOptions.ProcessingServiceConfiguration)
                        .Bind(configOptions);

                    services.AddSingleton(new MessageQueueClient(configOptions.QueueName, configOptions.MessageQueueConnectionString));
                });
    }
}
