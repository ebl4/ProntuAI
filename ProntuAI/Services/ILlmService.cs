using System.Collections.Generic;
using System.Threading.Tasks;
using ProntuAI.Models;

namespace ProntuAI.Services
{
    public interface ILlmService
    {
        Task<SoapNote> GenerateSoapAsync(string transcript, IEnumerable<string>? knowledgeIds = null);
    }
}
