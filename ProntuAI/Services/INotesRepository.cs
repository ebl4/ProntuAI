using ProntuAI.Models;

namespace ProntuAI.Services
{
    public interface INotesRepository
    {
        Task<string> SaveNoteAsync(SoapNote note, string? transcript = null);
        Task<IEnumerable<StoredNoteSummary>> ListNotesAsync(int limit = 50);
        Task<SoapNote?> GetNoteAsync(string id);
    }

    public record StoredNoteSummary(string Id, string CreatedAt, string SubjectivePreview);
}
