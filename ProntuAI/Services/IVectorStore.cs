namespace ProntuAI.Services
{
    // Minimal vector store abstraction for future RAG implementation.
    public interface IVectorStore
    {
        Task<string> AddDocumentAsync(string id, string content);
        Task<IEnumerable<string>> QueryAsync(string query, int topK = 3);
    }
}
