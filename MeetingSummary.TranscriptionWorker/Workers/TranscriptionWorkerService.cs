using Azure.Messaging.ServiceBus;
using MeetingSummary.Core.Interfaces;
using MeetingSummary.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MeetingSummary.TranscriptionWorker.BackgroundServices;

public class TranscriptionWorkerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public TranscriptionWorkerService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using AsyncServiceScope asyncScope = _scopeFactory.CreateAsyncScope();
        var serviceProvider = asyncScope.ServiceProvider;
        var logger = serviceProvider.GetRequiredService<ILogger<TranscriptionWorkerService>>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        var queueName = configuration["ServiceBus:MeetingChunkQueue"] ?? "meetingChunkQueue";
        var serviceBusConnectionstring = configuration["ServiceBus:ConnectionString"] ?? string.Empty;
        var checkInterval = TimeSpan.FromSeconds(5);

        var meetingChunkProcessor = serviceProvider.GetRequiredService<IMeetingChunkProcessor>();
        await using var client = new ServiceBusClient(serviceBusConnectionstring);
        var receiver = client.CreateReceiver(queueName);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {

                if (receiver == null)
                {
                    logger.LogError("Required services are not available. TranscriptionWorkerService cannot run.");
                    continue;
                }

                var meetingChunksToProcess = await receiver.ReceiveMessagesAsync(maxMessages: 1, cancellationToken: stoppingToken);

                if (meetingChunksToProcess == null || meetingChunksToProcess.Count <= 0)
                {
                    continue;
                }

                var meetingChunkModel = GetMeetingChunkModel(meetingChunksToProcess[0].Body.ToString());
                await meetingChunkProcessor.ProcessMessage(meetingChunkModel, stoppingToken);
                await receiver.CompleteMessageAsync(meetingChunksToProcess[0], stoppingToken);

                await Task.Delay(checkInterval, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while processing messages in background worker service");
            }
        }

    }

    public MeetingChunkModel GetMeetingChunkModel(string messageBody)
    {
        var messageModel = JsonConvert.DeserializeObject<MeetingChunkModel>(messageBody);
        return messageModel ?? new MeetingChunkModel();
    }
}


