using Microsoft.EntityFrameworkCore;
using ProntuAI.Data;
using ProntuAI.Entities;
using ProntuAI.Models;
using ProntuAI.Services;

namespace ProntuAI.Infrastructure.Repositories
{
    public class EFNotesRepository : INotesRepository
    {
        private readonly ApplicationDbContext _db;

        public EFNotesRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<string> SaveNoteAsync(SoapNote note, string? transcript = null)
        {
            var entity = new Note
            {
                Subjective = note.Subjective,
                Objective = note.Objective,
                Assessment = note.Assessment,
                Plan = note.Plan,
                Transcript = transcript
            };

            _db.Notes.Add(entity);
            await _db.SaveChangesAsync();
            return entity.Id.ToString();
        }

        public async Task<IEnumerable<StoredNoteSummary>> ListNotesAsync(int limit = 50)
        {
            return await _db.Notes
                .OrderByDescending(n => n.CreatedAt)
                .Take(limit)
                .Select(n => new StoredNoteSummary(n.Id.ToString(), n.CreatedAt.ToString("o"), (n.Subjective.Length > 120 ? n.Subjective.Substring(0, 120) + "..." : n.Subjective)))
                .ToListAsync();
        }

        public async Task<SoapNote?> GetNoteAsync(string id)
        {
            if (!Guid.TryParse(id, out var guid)) return null;
            var n = await _db.Notes.FindAsync(guid);
            if (n == null) return null;
            return new SoapNote { Subjective = n.Subjective, Objective = n.Objective, Assessment = n.Assessment, Plan = n.Plan };
        }
    }
}
