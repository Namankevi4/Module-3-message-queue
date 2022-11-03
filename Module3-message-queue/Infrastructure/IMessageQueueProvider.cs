using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using ModelObjects;

namespace Infrastructure
{
    public interface IMessageQueueProvider
    {
        MessageQueueClient CreateClient(string queueName, string connStr);
    }
}
