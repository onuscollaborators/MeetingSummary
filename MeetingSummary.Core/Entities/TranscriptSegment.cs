using System.Text.Json.Serialization;

namespace MeetingSummary.Core.Entities
{
    public class TranscriptSegment
    {
        [JsonPropertyName("id")]
        public string Id => $"{MeetingId}_{ChunkId}";

        [JsonPropertyName("meetingId")]
        public required string MeetingId { get; set; }

        [JsonPropertyName("chunkId")]
        public required string ChunkId { get; set; }

        [JsonPropertyName("transcriptText")]
        public required string TranscriptText { get; set; }

        [JsonPropertyName("language")]
        public required string Language { get; set; }

        [JsonPropertyName("chunkStartOffsetSec")]
        public required int ChunkStartOffsetSec { get; set; }

        [JsonPropertyName("chunkEndOffsetSec")]
        public required int ChunkEndOffsetSec { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
