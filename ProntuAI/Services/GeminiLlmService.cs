using System.Text.Json;
using ProntuAI.Models;

namespace ProntuAI.Services
{
    // Minimal Gemini client wrapper - expects GEMINI_API_KEY and GEMINI_ENDPOINT env vars.
    public class GeminiLlmService : ILlmService
    {
        private readonly HttpClient _http;
        private readonly string? _apiKey;
        private readonly string? _endpoint;

        public GeminiLlmService(HttpClient http)
        {
            _http = http;
            _apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
            _endpoint = Environment.GetEnvironmentVariable("GEMINI_ENDPOINT");
        }

        public async Task<SoapNote> GenerateSoapAsync(string transcript, IEnumerable<string>? knowledgeIds = null)
        {
            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_endpoint))
                throw new InvalidOperationException("Gemini not configured. Set GEMINI_API_KEY and GEMINI_ENDPOINT.");

            var prompt = BuildPrompt(transcript, knowledgeIds);

            var reqObj = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            using var req = new HttpRequestMessage(HttpMethod.Post, _endpoint)
            {
                Content = JsonContent.Create(reqObj)
            };

            req.Headers.Add("X-goog-api-key", _apiKey);

            var resp = await _http.SendAsync(req);
            resp.EnsureSuccessStatusCode();

            var body = await resp.Content.ReadAsStringAsync();

            // Expecting LLM to return a JSON object with fields Subjective, Objective, Assessment, Plan
            try
            {
                var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                // Gemini may return { "candidates": [ { "content": { "parts": [ { "text": "..." } ] } } ] }
                if (root.ValueKind == JsonValueKind.Object)
                {
                    if (root.TryGetProperty("candidates", out var candidates) && candidates.ValueKind == JsonValueKind.Array && candidates.GetArrayLength() > 0)
                    {
                        var first = candidates[0];
                        if (first.ValueKind == JsonValueKind.Object && first.TryGetProperty("content", out var contentObj))
                        {
                            // content.parts[*].text
                            if (contentObj.ValueKind == JsonValueKind.Object && contentObj.TryGetProperty("parts", out var parts) && parts.ValueKind == JsonValueKind.Array)
                            {
                                var sb = new System.Text.StringBuilder();
                                foreach (var part in parts.EnumerateArray())
                                {
                                    if (part.ValueKind == JsonValueKind.Object && part.TryGetProperty("text", out var txt) && txt.ValueKind == JsonValueKind.String)
                                    {
                                        sb.Append(txt.GetString());
                                    }
                                }
                                var combined = sb.ToString();
                                if (!string.IsNullOrEmpty(combined)) return ParseSoapJson(combined);
                            }

                            // fallback: content could be string
                            if (contentObj.ValueKind == JsonValueKind.String)
                            {
                                var contentStr = contentObj.GetString();
                                if (!string.IsNullOrEmpty(contentStr)) return ParseSoapJson(contentStr);
                            }
                        }
                    }

                    // If the model wrapped JSON in content at root level
                    if (root.TryGetProperty("content", out var content))
                    {
                        if (content.ValueKind == JsonValueKind.String) return ParseSoapJson(content.GetString() ?? string.Empty);
                    }
                }

                return ParseSoapJson(body);
            }
            catch (JsonException)
            {
                // Try to extract JSON substring
                var json = ExtractJsonSubstring(body);
                if (!string.IsNullOrEmpty(json)) return ParseSoapJson(json);
                throw new InvalidOperationException("LLM response not valid JSON for SOAP note.");
            }
        }

        private string BuildPrompt(string transcript, IEnumerable<string>? knowledgeIds)
        {
            // Prompt template instructing the model to produce strict JSON with SOAP fields
            var header = "You are a clinical assistant. Given the transcript of a medical encounter and optional knowledge snippets, produce a JSON object with the following fields: Subjective, Objective, Assessment, Plan. Output MUST be valid JSON only (no extra commentary). Use Portuguese (pt-BR).";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine(header);
            if (knowledgeIds != null)
            {
                sb.AppendLine("KnowledgeIds:");
                foreach (var id in knowledgeIds) sb.AppendLine(id);
            }
            sb.AppendLine("---TRANSCRIPT---");
            sb.AppendLine(transcript);
            sb.AppendLine("---END---");
            sb.AppendLine("Respond with JSON object like { \"Subjective\": \"...\", \"Objective\": \"...\", \"Assessment\": \"...\", \"Plan\": \"...\" }");

            return sb.ToString();
        }

        private SoapNote ParseSoapJson(string json)
        {
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var dto = JsonSerializer.Deserialize<SoapNote>(json, opts);
            if (dto == null) throw new InvalidOperationException("Failed to parse SOAP JSON from LLM.");
            return dto;
        }

        private string ExtractJsonSubstring(string text)
        {
            var start = text.IndexOf('{');
            var end = text.LastIndexOf('}');
            if (start >= 0 && end > start) return text.Substring(start, end - start + 1);
            return string.Empty;
        }
    }
}
