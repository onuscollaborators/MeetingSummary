namespace MeetingSummary.Core.Models
{
    public record MeetingChunkModel
    {
        public string MeetingId { get; set; } = default!;
        public string ChunkId { get; set; } = default!;
        public string BlobUrl { get; set; } = default!;
        public string Language { get; set; } = default!;
        public int ChunkStartOffsetSec { get; set; }
        public int ChunkEndOffsetSec { get; set; }
    }
}
