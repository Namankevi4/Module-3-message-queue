using System;
using System.Collections.Generic;
using System.Text;
using Infrastructure;
using ModelObjects;

namespace Infrastructure
{
    public class MessageQueueProvider: IMessageQueueProvider
    {
        public MessageQueueClient CreateClient(string queueName, string connStr)
        {
            return new MessageQueueClient(queueName, connStr);
        }
    }
}
