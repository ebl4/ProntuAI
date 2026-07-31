using System.Text.Json;
using ProntuAI.Models;

namespace ProntuAI.Services
{
    public class FileNotesRepository : INotesRepository
    {
        private readonly string _baseDir;

        public FileNotesRepository()
        {
            _baseDir = Path.Combine(Path.GetTempPath(), "ProntuAI", "notes");
            Directory.CreateDirectory(_baseDir);
        }

        public async Task<string> SaveNoteAsync(SoapNote note, string? transcript = null)
        {
            var id = Guid.NewGuid().ToString();
            var wrapper = new
            {
                id,
                createdAt = DateTime.UtcNow,
                note,
                transcript
            };

            var path = Path.Combine(_baseDir, id + ".json");
            var opts = new JsonSerializerOptions { WriteIndented = true };
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(wrapper, opts));
            return id;
        }

        public Task<IEnumerable<StoredNoteSummary>> ListNotesAsync(int limit = 50)
        {
            var files = new DirectoryInfo(_baseDir).GetFiles("*.json")
                .OrderByDescending(f => f.CreationTimeUtc)
                .Take(limit);

            var summaries = files.Select(f =>
            {
                try
                {
                    var json = File.ReadAllText(f.FullName);
                    using var doc = JsonDocument.Parse(json);
                    var id = doc.RootElement.GetProperty("id").GetString() ?? f.Name;
                    var created = doc.RootElement.GetProperty("createdAt").GetDateTime().ToString("o");
                    var subj = doc.RootElement.GetProperty("note").GetProperty("Subjective").GetString() ?? string.Empty;
                    var preview = subj.Length > 120 ? subj.Substring(0, 120) + "..." : subj;
                    return new StoredNoteSummary(id, created, preview);
                }
                catch
                {
                    return new StoredNoteSummary(f.Name, f.CreationTimeUtc.ToString("o"), string.Empty);
                }
            });

            return Task.FromResult(summaries);
        }

        public async Task<SoapNote?> GetNoteAsync(string id)
        {
            var path = Path.Combine(_baseDir, id + ".json");
            if (!File.Exists(path)) return null;
            var json = await File.ReadAllTextAsync(path);
            using var doc = JsonDocument.Parse(json);
            var node = doc.RootElement.GetProperty("note");
            var note = JsonSerializer.Deserialize<SoapNote>(node.GetRawText());
            return note;
        }
    }
}
