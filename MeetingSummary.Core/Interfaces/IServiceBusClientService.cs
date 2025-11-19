namespace MeetingSummary.Core.Interfaces
{
    public interface IServiceBusClientService
    {
        Task<T> ReadMessageAsync<T>(string connectionString, string queueName) where T : class;
        Task<List<T>> ReadMessageInBatchAsync<T>(string connectionString, string queueName, int messageCount) where T : class;
        Task SendMessageAsync(object model, string connectionString, string queueName);
    }
}
