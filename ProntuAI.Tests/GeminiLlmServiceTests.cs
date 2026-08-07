using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using ProntuAI.Services;
using Xunit;

namespace ProntuAI.Tests
{
    public class GeminiLlmServiceTests
    {
        private class FakeHandler : HttpMessageHandler
        {
            private readonly string _responseContent;
            public FakeHandler(string responseContent)
            {
                _responseContent = responseContent;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(_responseContent)
                };
                resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return Task.FromResult(resp);
            }
        }

        [Fact]
        public async Task GenerateSoapAsync_Parses_Gemini_Candidates_Parts_Text()
        {
            // Arrange: Gemini-like wrapper where parts[].text contains the JSON string
            // Build inner JSON string and wrap it in Gemini response shape
            var innerJson = System.Text.Json.JsonSerializer.Serialize(new { Subjective = "S", Objective = "O", Assessment = "A", Plan = "P" });
            var geminiRespObj = new { candidates = new[] { new { content = new { parts = new[] { new { text = innerJson } } } } } };
            var geminiResp = System.Text.Json.JsonSerializer.Serialize(geminiRespObj);

            // Create HttpClient with fake handler
            var handler = new FakeHandler(geminiResp);
            var http = new HttpClient(handler);

            // Ensure environment variables expected by the service
            System.Environment.SetEnvironmentVariable("GEMINI_API_KEY", "fake-key");
            System.Environment.SetEnvironmentVariable("GEMINI_ENDPOINT", "https://api.fake.gemini");

            var svc = new GeminiLlmService(http);

            // Act
            var note = await svc.GenerateSoapAsync("transcript text");

            // Assert
            Assert.NotNull(note);
            Assert.Equal("S", note.Subjective);
            Assert.Equal("O", note.Objective);
            Assert.Equal("A", note.Assessment);
            Assert.Equal("P", note.Plan);
        }
    }
}
