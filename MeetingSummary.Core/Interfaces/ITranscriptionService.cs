using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingSummary.Core.Interfaces
{
    public interface ITranscriptionService
    {
        Task<string> TranscribeAudio(string blobUrl, string language);
    }
}
