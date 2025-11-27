using MeetingSummary.Core.Models;

namespace MeetingSummary.Core.Interfaces
{
    public interface IMeetingChunkProcessor
    {
        Task ProcessMessage(MeetingChunkModel message, CancellationToken cancellationToken = default);
    }
}
