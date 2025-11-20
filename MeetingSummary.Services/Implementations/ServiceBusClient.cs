using Azure.Messaging.ServiceBus;
using MeetingSummary.Core.Interfaces;
using Newtonsoft.Json;

namespace MeetingSummary.Services.Implementations;

public class ServiceBusClientService : IServiceBusClientService
{
    public async Task SendMessageAsync(object model, string connectionString, string queueName)
    {
        await using var client = new ServiceBusClient(connectionString);
        ServiceBusSender sender = client.CreateSender(queueName);
        var messageBody = JsonConvert.SerializeObject(model);
        var message = new ServiceBusMessage(messageBody);

        await sender.SendMessageAsync(message);
    }
    public async Task<T> ReadMessageAsync<T>(string connectionString, string queueName) where T : class
    {
        await using (var client = new ServiceBusClient(connectionString))
        {
            var receiver = client.CreateReceiver(queueName);
            var messages = await receiver.ReceiveMessagesAsync(1);

            if (messages?.Any() == false)
                return default;

            string body = messages.First().Body.ToString();
            await receiver.CompleteMessageAsync(messages.First());

            var messageModel = JsonConvert.DeserializeObject<T>(body);

            return messageModel;
        }
    }
    public async Task<List<T>> ReadMessageInBatchAsync<T>(string connectionString, string queueName, int messageCount) where T : class
    {
        await using (var client = new ServiceBusClient(connectionString))
        {
            var receiver = client.CreateReceiver(queueName);
            var messages = await receiver.ReceiveMessagesAsync(messageCount);

            if (messages?.Any() == false)
                return [];

            List<T> messageList = new();
            foreach (ServiceBusReceivedMessage message in messages)
            {
                string body = message.Body.ToString();
                var messageModel = JsonConvert.DeserializeObject<T>(body);
                messageList.Add(messageModel);

                await receiver.CompleteMessageAsync(message);
            }
           

            return messageList;
        }
    }
}
