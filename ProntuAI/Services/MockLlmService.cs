using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ProntuAI.Models;

namespace ProntuAI.Services
{
    // Mock LLM service that generates a basic SOAP note from a transcript.
    public class MockLlmService : ILlmService
    {
        public Task<SoapNote> GenerateSoapAsync(string transcript, IEnumerable<string>? knowledgeIds = null)
        {
            if (string.IsNullOrWhiteSpace(transcript))
                throw new ArgumentException("Transcript is empty", nameof(transcript));

            // Simple heuristics to split subjective/objective
            var subjective = Truncate(transcript, 800);
            var objective = ExtractVitals(transcript);
            var assessment = "Hipótese diagnóstica sugerida: cefaleia tensional / enxaqueca. Avaliar sinais de alarme se novos sintomas surgirem.";
            var plan = "Orientações iniciais: analgesia conforme dor, observar sinais de alarme, retorno se piora. Considerar imagem se sinais neurológicos presentes.";

            var soap = new SoapNote
            {
                Subjective = subjective,
                Objective = objective,
                Assessment = assessment,
                Plan = plan
            };

            return Task.FromResult(soap);
        }

        private static string Truncate(string s, int max)
        {
            if (s.Length <= max) return s;
            return s.Substring(0, max) + "...";
        }

        private static string ExtractVitals(string text)
        {
            // Very naive extraction: look for numbers followed by 'kg', 'cm', 'mmHg', 'bpm', '°C'
            var matches = Regex.Matches(text, "\\d+\\s?(kg|cm|mmHg|bpm|°C)", RegexOptions.IgnoreCase);
            if (matches.Count == 0) return string.Empty;
            return string.Join("; ", matches.Select(m => m.Value));
        }
    }
}
