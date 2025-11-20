using MeetingSummary.Core.Entities;

namespace MeetingSummary.Core.Interfaces
{
    public interface ITranscriptRepository
    {
        Task UpsertTranscriptSegment(TranscriptSegment segment, CancellationToken cancellationToken = default);
        Task<TranscriptSegment?> GetTranscriptSegment(string meetingId, string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<TranscriptSegment>> GetMeetingTranscripts(string meetingId, CancellationToken cancellationToken = default);
    }

}
