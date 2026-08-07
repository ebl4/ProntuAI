using System.ComponentModel.DataAnnotations;

namespace ProntuAI.Entities
{
    public class AudioFile
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long Size { get; set; }
        public string? StoredPath { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public string UploadedById { get; set; } = string.Empty;
    }
}
