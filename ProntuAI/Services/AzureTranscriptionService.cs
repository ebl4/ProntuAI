using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProntuAI.Services
{
    // Minimal Azure Speech-to-Text implementation using REST API.
    // Requires AZURE_SPEECH_KEY and AZURE_SPEECH_REGION environment variables.
    public class AzureTranscriptionService : ITranscriptionService
    {
        private readonly HttpClient _http;
        private readonly string? _key;
        private readonly string? _region;

        public AzureTranscriptionService(HttpClient http)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _key = Environment.GetEnvironmentVariable("AZURE_SPEECH_KEY");
            _region = Environment.GetEnvironmentVariable("AZURE_SPEECH_REGION");
        }

        public async Task<string> TranscribeAsync(Stream audioStream, string contentType)
        {
            if (string.IsNullOrEmpty(_key) || string.IsNullOrEmpty(_region))
            {
                // Fallback to mock behaviour when not configured
                return await new MockTranscriptionService().TranscribeAsync(audioStream, contentType);
            }

            // Azure Speech REST: create a request to the speech-to-text endpoint
            var uri = new Uri($"https://{_region}.stt.speech.microsoft.com/speech/recognition/conversation/cognitiveservices/v1?language=pt-BR");

            using var request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Add("Ocp-Apim-Subscription-Key", _key);
            request.Content = new StreamContent(audioStream);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType ?? "audio/wav");

            using var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadAsStringAsync();
            try
            {
                using var doc = JsonDocument.Parse(payload);
                if (doc.RootElement.TryGetProperty("DisplayText", out var dt))
                    return dt.GetString() ?? string.Empty;

                // Some endpoints respond with 'NBest' structure; try to extract common fields
                if (doc.RootElement.TryGetProperty("RecognitionStatus", out var _))
                {
                    if (doc.RootElement.TryGetProperty("DisplayText", out var d)) return d.GetString() ?? string.Empty;
                }
            }
            catch
            {
                // Not JSON? return raw
            }

            return payload;
        }
    }
}
