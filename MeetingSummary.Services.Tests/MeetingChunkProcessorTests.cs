using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MeetingSummary.Core.Entities;
using MeetingSummary.Core.Interfaces;
using MeetingSummary.Core.Models;
using MeetingSummary.Services.Implementations;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeetingSummary.Services.Tests
{
    public class MeetingChunkProcessorTests
    {
        private readonly Mock<ITranscriptionService> _mockTranscriptionService;
        private readonly Mock<ITranscriptRepository> _mockTranscriptRepository;
        private readonly Mock<ILogger<MeetingChunkProcessor>> _mockLogger;
        private readonly Mock<IValidator<MeetingChunkModel>> _mockValidator;
        private readonly MeetingChunkProcessor _processor;

        public MeetingChunkProcessorTests()
        {
            _mockTranscriptionService = new Mock<ITranscriptionService>();
            _mockTranscriptRepository = new Mock<ITranscriptRepository>();
            _mockLogger = new Mock<ILogger<MeetingChunkProcessor>>();
            _mockValidator = new Mock<IValidator<MeetingChunkModel>>();

            _processor = new MeetingChunkProcessor(
                _mockTranscriptionService.Object,
                _mockTranscriptRepository.Object,
                _mockLogger.Object,
                _mockValidator.Object);
        }

        [Fact]
        public async Task ProcessMessage_WithValidMessage_ShouldProcessSuccessfully()
        {
            // Arrange
            var message = new MeetingChunkModel
            {
                MeetingId = "meeting-123",
                ChunkId = "chunk-001",
                BlobUrl = "https://example.com/audio.wav",
                Language = "en-US",
                ChunkStartOffsetSec = 0,
                ChunkEndOffsetSec = 60
            };

            var transcribedText = "This is the transcribed text.";

            _mockValidator
                .Setup(v => v.ValidateAsync(message, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockTranscriptionService
                .Setup(s => s.TranscribeAudio(message.BlobUrl, message.Language))
                .ReturnsAsync(transcribedText);

            _mockTranscriptRepository
                .Setup(r => r.UpsertTranscriptSegment(It.IsAny<TranscriptSegment>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _processor.ProcessMessage(message);

            // Assert
            _mockValidator.Verify(v => v.ValidateAsync(message, It.IsAny<CancellationToken>()), Times.Once);
            _mockTranscriptionService.Verify(s => s.TranscribeAudio(message.BlobUrl, message.Language), Times.Once);
            _mockTranscriptRepository.Verify( r => r.UpsertTranscriptSegment(It.Is<TranscriptSegment>(seg => seg.Id == "meeting-123-chunk-001"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ProcessMessage_WithValidationFailure_ShouldThrowValidationException()
        {
            // Arrange
            var message = new MeetingChunkModel
            {
                MeetingId = "",
                ChunkId = "chunk-001",
                BlobUrl = "https://example.com/audio.wav",
                Language = "en-US",
                ChunkStartOffsetSec = 0,
                ChunkEndOffsetSec = 60
            };

            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure("MeetingId", "MeetingId is required")
            };

            var validationResult = new ValidationResult(validationFailures);

            _mockValidator
                .Setup(v => v.ValidateAsync(message, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            // Act
            var act = async () => await _processor.ProcessMessage(message);

            // Assert
            await act.Should().ThrowAsync<ValidationException>().WithMessage("*MeetingId is required*");

            _mockTranscriptionService.Verify( s => s.TranscribeAudio(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockTranscriptRepository.Verify( r => r.UpsertTranscriptSegment(It.IsAny<TranscriptSegment>(), It.IsAny<CancellationToken>()),Times.Never);
        }

    }
}
