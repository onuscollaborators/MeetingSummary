using FluentValidation;
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
        private readonly IValidator<MeetingChunkModel> _validator;

        public MeetingChunkProcessor(
            ITranscriptionService transcriptionService,
            ITranscriptRepository transcriptRepository,
            ILogger<MeetingChunkProcessor> logger,
            IValidator<MeetingChunkModel> validator)
        {
            _transcriptionService = transcriptionService;
            _transcriptRepository = transcriptRepository;
            _logger = logger;
            _validator = validator;
        }

        public async Task ProcessMessage(MeetingChunkModel message, CancellationToken cancellationToken = default)
        {
            try
            {
                var validationResult = await _validator.ValidateAsync(message, cancellationToken);
                if (!validationResult.IsValid)
                {
                    var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                    _logger.LogError("Validation failed error {Errors}", errors);

                    throw new ValidationException(validationResult.Errors);
                }

                var id = CreateId(message.MeetingId, message.ChunkId);
                var transcriptText = await _transcriptionService.TranscribeAudio(message.BlobUrl, message.Language);

                var segment = new TranscriptSegment
                {
                    Id = id,
                    MeetingId = message.MeetingId,
                    ChunkId = message.ChunkId,
                    TranscriptText = transcriptText,
                    Language = message.Language,
                    ChunkStartOffsetSec = message.ChunkStartOffsetSec,
                    ChunkEndOffsetSec = message.ChunkEndOffsetSec,
                    CreatedAt = DateTime.UtcNow.ToString("o"),
                    UpdatedAt = DateTime.UtcNow.ToString("o")
                };

                await _transcriptRepository.UpsertTranscriptSegment(segment, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing queue meeting chunk for meeting {MeetingId}, chunk {ChunkId}", message.MeetingId, message.ChunkId);
                throw;
            }
        }

        private string CreateId(string meetingId, string chunkId) => $"{meetingId}-{chunkId}";
    }
}
