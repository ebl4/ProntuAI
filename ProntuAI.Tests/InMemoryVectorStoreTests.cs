using System.Linq;
using System.Threading.Tasks;
using ProntuAI.Services;
using Xunit;

namespace ProntuAI.Tests
{
    public class InMemoryVectorStoreTests
    {
        [Fact]
        public async Task AddAndQuery_ReturnsDocumentId()
        {
            var store = new InMemoryVectorStore();
            await store.AddDocumentAsync("doc1", "Protocolo de hipertensão: medir PA, orientar dieta.");
            var results = await store.QueryAsync("hipertensão");
            Assert.Contains("doc1", results);
        }
    }
}
