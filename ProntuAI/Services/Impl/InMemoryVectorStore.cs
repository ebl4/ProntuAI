using System.Collections.Concurrent;

namespace ProntuAI.Services.Impl
{
    // Very simple in-memory 'vector store' for MVP that stores documents and performs naive text matching.
    public class InMemoryVectorStore : IVectorStore
    {
        private readonly ConcurrentDictionary<string, string> _docs = new();

        public Task<string> AddDocumentAsync(string id, string content)
        {
            _docs[id] = content ?? string.Empty;
            return Task.FromResult(id);
        }

        public Task<IEnumerable<string>> QueryAsync(string query, int topK = 3)
        {
            if (string.IsNullOrWhiteSpace(query)) return Task.FromResult(Enumerable.Empty<string>());

            var terms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var scores = _docs.Select(kv => new
            {
                Id = kv.Key,
                Score = terms.Sum(t => kv.Value.Contains(t, StringComparison.OrdinalIgnoreCase) ? 1 : 0)
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .Take(topK)
            .Select(x => x.Id);

            return Task.FromResult(scores);
        }
    }
}
