using System.ComponentModel.DataAnnotations;

namespace ProntuAI.Entities
{
    public class Note
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Subjective { get; set; } = string.Empty;
        public string Objective { get; set; } = string.Empty;
        public string Assessment { get; set; } = string.Empty;
        public string Plan { get; set; } = string.Empty;
        public string? Transcript { get; set; }
        public string CreatedById { get; set; } = string.Empty; // Identity user id
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
