using System.IO;
using System.Threading.Tasks;

namespace ProntuAI.Services
{
    public interface ITranscriptionService
    {
        Task<string> TranscribeAsync(Stream audioStream, string contentType);
    }
}
