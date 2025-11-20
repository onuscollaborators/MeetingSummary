using MeetingSummary.Core.Entities;
using MeetingSummary.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

namespace MeetingSummary.Services.Implementations
{
    public class CosmosDbTranscriptRepository : ITranscriptRepository
    {
        private readonly Container _container;
        private readonly ILogger<CosmosDbTranscriptRepository> _logger;

        public CosmosDbTranscriptRepository(
            CosmosClient cosmosClient,
            IConfiguration configuration,
            ILogger<CosmosDbTranscriptRepository> logger)
        {
            _logger = logger;

            var databaseName = configuration["CosmosDb:DatabaseName"]
                ?? throw new InvalidOperationException("CosmosDb:DatabaseName cannot be found in configuration");

            var containerName = configuration["CosmosDb:ContainerName"]
                ?? throw new InvalidOperationException("CosmosDb:ContainerName cannot be found in configuration");

            _container = cosmosClient.GetContainer(databaseName, containerName);
        }

        public async Task UpsertTranscriptSegment(TranscriptSegment segment, CancellationToken cancellationToken = default)
        {
            try
            {
                segment.UpdatedAt = DateTime.UtcNow.ToString("o");

                var response = await _container.UpsertItemAsync<TranscriptSegment>(
                   segment,
                    new PartitionKey(segment.MeetingId)
                );
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, $"Error upserting transcript segment {segment.ChunkId}");
                throw;
            }
        }

        public async Task<TranscriptSegment?> GetTranscriptSegment(string meetingId, string id, CancellationToken cancellationToken = default)
        {
            try
            {

                var response = await _container.ReadItemAsync<TranscriptSegment>(
                    id: id,
                    partitionKey: new PartitionKey(meetingId),
                    cancellationToken: cancellationToken
                );

                return response.Resource;
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Error retrieving transcript segment Id {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<TranscriptSegment>> GetMeetingTranscripts(string meetingId, CancellationToken cancellationToken = default)
        {
            try
            {
                var query = new QueryDefinition(
                    "SELECT * FROM c WHERE c.meetingId = @meetingId ORDER BY c.chunkStartOffsetSec")
                    .WithParameter("@meetingId", meetingId);

                var iterator = _container.GetItemQueryIterator<TranscriptSegment>(
                    queryDefinition: query,
                    requestOptions: new QueryRequestOptions
                    {
                        PartitionKey = new PartitionKey(meetingId)
                    }
                );

                var results = new List<TranscriptSegment>();
                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync(cancellationToken);
                    results.AddRange(response);
                }

                return results;
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Error retrieving transcript segments for meeting {MeetingId}", meetingId);
                throw;
            }
        }
    }
}
