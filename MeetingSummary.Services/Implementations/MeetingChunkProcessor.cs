using MeetingSummary.Core.Entities;
using MeetingSummary.Core.Interfaces;
using MeetingSummary.Core.Models;
using Microsoft.Extensions.Logging;

namespace MeetingSummary.Services.Implementations
{
    public class MeetingChunkProcessor : IMeetingChunkProcessor
    {
        private readonly ITranscriptionService _transcriptionService;
        private readonly ITranscriptRepository _transcriptRepository;
        private readonly ILogger<MeetingChunkProcessor> _logger;

        public MeetingChunkProcessor(
            ITranscriptionService transcriptionService,
            ITranscriptRepository transcriptRepository,
            ILogger<MeetingChunkProcessor> logger)
        {
            _transcriptionService = transcriptionService;
            _transcriptRepository = transcriptRepository;
            _logger = logger;
        }

        public async Task ProcessMessage(MeetingChunkModel message, CancellationToken cancellationToken = default)
        {
            try
            {
                var existingSegment = await _transcriptRepository
                    .GetTranscriptSegment(message.MeetingId, message.ChunkId, cancellationToken);

                var transcriptText = await _transcriptionService.TranscribeAudio(message.BlobUrl, message.Language);

                var segment = new TranscriptSegment
                {
                    MeetingId = message.MeetingId,
                    ChunkId = message.ChunkId,
                    TranscriptText = transcriptText,
                    Language = message.Language,
                    ChunkStartOffsetSec = message.ChunkStartOffsetSec,
                    ChunkEndOffsetSec = message.ChunkEndOffsetSec,
                    CreatedAt = existingSegment?.CreatedAt ?? DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _transcriptRepository.UpsertTranscriptSegment(segment, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing queue meeting chunk for meeting {MeetingId}, chunk {ChunkId}", message.MeetingId, message.ChunkId);
                throw;
            }
        }
    }
}
