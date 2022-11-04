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

namespace CaptureService
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
                    services.Configure<CaptureServiceOptions>(
                        hostContext.Configuration.GetSection(CaptureServiceOptions.CaptureServiceConfiguration));
                    services.AddSingleton<IFileReaderService, FileReaderService>();

                    var configOptions = new CaptureServiceOptions();
                    hostContext.Configuration.GetSection(CaptureServiceOptions.CaptureServiceConfiguration)
                        .Bind(configOptions);

                    services.AddSingleton(new MessageQueueClient(configOptions.QueueName, configOptions.MessageQueueConnectionString));
                    services.AddHostedService<Worker>();
                });
    }
}
