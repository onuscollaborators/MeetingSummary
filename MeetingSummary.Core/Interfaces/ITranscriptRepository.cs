using MeetingSummary.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingSummary.Core.Interfaces
{
    public interface ITranscriptRepository
    {
        Task UpsertTranscriptSegment(TranscriptSegment segment, CancellationToken cancellationToken = default);
        Task<TranscriptSegment?> GetTranscriptSegment(string meetingId, string chunkId, CancellationToken cancellationToken = default);
        Task<IEnumerable<TranscriptSegment>> GetMeetingTranscripts(string meetingId, CancellationToken cancellationToken = default);
    }

}
