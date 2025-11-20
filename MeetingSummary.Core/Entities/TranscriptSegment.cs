using System.Text.Json.Serialization;

namespace MeetingSummary.Core.Entities
{
    public class TranscriptSegment
    {
        [JsonPropertyName("id")]
        public required string Id { get; set; } = default!;

        [JsonPropertyName("meetingId")]
        public required string MeetingId { get; set; }

        [JsonPropertyName("chunkId")]
        public required string ChunkId { get; set; }

        [JsonPropertyName("transcriptText")]
        public string TranscriptText { get; set; } = default!;

        [JsonPropertyName("language")]
        public  string Language { get; set; } = default!;

        [JsonPropertyName("chunkStartOffsetSec")]
        public  int ChunkStartOffsetSec { get; set; } = default!;

        [JsonPropertyName("chunkEndOffsetSec")]
        public  int ChunkEndOffsetSec { get; set; } = default!;

        [JsonPropertyName("createdAt")]
        public string CreatedAt { get; set; } = default!;

        [JsonPropertyName("updatedAt")]
        public string UpdatedAt { get; set; } = default!;
    }
}
