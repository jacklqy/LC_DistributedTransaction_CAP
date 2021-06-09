using DotNetCore.CAP.Messages;
using DotNetCore.CAP.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Zhaoxi.DistributedTransaction.Utility
{
    /// <summary>
    /// 自定义序列化器
    /// </summary>
    public class CustomSerializer : ISerializer
    {
        public Task<TransportMessage> SerializeAsync(Message message)
        {
            Console.WriteLine("This is CustomSerializer...........");
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (message.Value == null)
            {
                return Task.FromResult(new TransportMessage(message.Headers, null));
            }

            var json = JsonConvert.SerializeObject(message.Value);
            return Task.FromResult(new TransportMessage(message.Headers, Encoding.UTF8.GetBytes(json)));
        }

        public Task<Message> DeserializeAsync(TransportMessage transportMessage, Type valueType)
        {
            Console.WriteLine("This is CustomSerializer...........");

            if (valueType == null || transportMessage.Body == null)
            {
                return Task.FromResult(new Message(transportMessage.Headers, null));
            }

            var json = Encoding.UTF8.GetString(transportMessage.Body);
            return Task.FromResult(new Message(transportMessage.Headers, JsonConvert.DeserializeObject(json, valueType)));
        }
    }
}
