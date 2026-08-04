using ProntuAI.Services;
using Xunit;

namespace ProntuAI.Tests
{
    public class GeminiPromptTests
    {
        [Fact]
        public void BuildPrompt_IncludesTranscript()
        {
            var svc = new GeminiLlmService(new System.Net.Http.HttpClient());
            var method = typeof(GeminiLlmService).GetMethod("BuildPrompt", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var prompt = method!.Invoke(svc, new object[] { "Paciente com dor no peito", null }) as string;
            Assert.NotNull(prompt);
            Assert.Contains("Paciente com dor no peito", prompt);
        }
    }
}
