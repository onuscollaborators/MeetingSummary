using MeetingSummary.Core.Interfaces;
namespace MeetingSummary.Services.Implementations
{
    public class StubTranscriptionService : ITranscriptionService
    {
        public async Task<string> TranscribeAudio(string blobUrl, string language)
        {
            //TODO call transcription API service

            await Task.CompletedTask;
            return "Mock transcript: Hello everyone, welcome to today's meeting. Let's get started with the agenda.";
        }
    }
}
