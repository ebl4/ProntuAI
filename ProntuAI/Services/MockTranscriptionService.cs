using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ProntuAI.Services
{
    // Mock implementation for development. Replace with real provider (Whisper/Azure) later.
    public class MockTranscriptionService : ITranscriptionService
    {
        public async Task<string> TranscribeAsync(Stream audioStream, string contentType)
        {
            // If the uploaded content is text (for easier testing), read it and return directly.
            if (!string.IsNullOrEmpty(contentType) && contentType.StartsWith("text/"))
            {
                using var reader = new StreamReader(audioStream, Encoding.UTF8);
                var text = await reader.ReadToEndAsync();
                return text;
            }

            // For binary audio, return a deterministic placeholder for MVP.
            return "Transcrição de exemplo: paciente relata cefaleia intensa há 2 dias, náusea ocasional, sem febre. História pregressa de hipertensão. Medicação em uso: losartana.";
        }
    }
}
