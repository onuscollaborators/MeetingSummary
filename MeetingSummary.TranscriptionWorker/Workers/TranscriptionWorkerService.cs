using MeetingSummary.Core.Interfaces;
using MeetingSummary.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
namespace MeetingSummary.TranscriptionWorker.BackgroundServices
{
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

            var serviceBusClientService = serviceProvider.GetRequiredService<IServiceBusClientService>();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (serviceBusClientService == null)
                    {
                        logger.LogError("Required services are not available. TranscriptionWorkerService cannot run.");
                        continue;
                    }

                    var meetingChunksToProcess = await serviceBusClientService
                        .ReadMessageInBatchAsync<MeetingChunkModel>(serviceBusConnectionstring, queueName, 1);

                    if (meetingChunksToProcess == null || meetingChunksToProcess.Count <= 0)
                    {
                        continue;
                    }

                    var meetingChunk = meetingChunksToProcess[0];

                    var meetingChunkProcessor = serviceProvider.GetRequiredService<IMeetingChunkProcessor>();
                    await meetingChunkProcessor.ProcessMessage(meetingChunk, stoppingToken);

                    await Task.Delay(checkInterval, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error occurred while processing messages in background worker service");
                }
            }

        }
    }
}
