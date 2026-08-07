using Microsoft.AspNetCore.Mvc;
using ProntuAI.Services;
using ProntuAI.Models;

namespace ProntuAI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TranscriptionController : ControllerBase
    {
        private readonly ITranscriptionService _transcriptionService;
        private readonly ILlmService _llmService;
        private readonly INotesRepository _notesRepository;
        private readonly ILogger<TranscriptionController> _logger;

        public TranscriptionController(ITranscriptionService transcriptionService, ILlmService llmService, INotesRepository notesRepository, ILogger<TranscriptionController> logger)
        {
            _transcriptionService = transcriptionService;
            _llmService = llmService;
            _notesRepository = notesRepository;
            _logger = logger;
        }

        [HttpPost("upload")]
        [RequestSizeLimit(524288000)]
        [RequestFormLimits(MultipartBodyLengthLimit = 524288000)]
        public async Task<IActionResult> UploadAudio([FromForm] IFormFile file)
        {
            if (file == null) return BadRequest("Arquivo não enviado.");
            // Save uploaded file to a temp folder for audit/processing
            var uploadsDir = Path.Combine(Path.GetTempPath(), "ProntuAI", "uploads");
            Directory.CreateDirectory(uploadsDir);
            var id = Guid.NewGuid().ToString();
            var filePath = Path.Combine(uploadsDir, id + Path.GetExtension(file.FileName));

            await using (var fs = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(fs);
            }

            // For immediate response, transcribe from the saved file stream
            await using var stream = System.IO.File.OpenRead(filePath);
            var transcript = await _transcriptionService.TranscribeAsync(stream, file.ContentType);

            return Ok(new { id, transcript });
        }

        [HttpPost("generate-soap")]
        public async Task<IActionResult> GenerateSoap([FromBody] GenerateSoapRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Transcript)) return BadRequest("Transcript required.");

            var soap = await _llmService.GenerateSoapAsync(req.Transcript, req.KnowledgeIds);
            return Ok(soap);
        }

        [HttpPost("save-note")]
        public async Task<IActionResult> SaveNote([FromBody] SaveNoteRequest req)
        {
            if (req?.Note == null) return BadRequest("Note required.");

            var id = await _notesRepository.SaveNoteAsync(req.Note, req.Transcript);
            return Ok(new { id });
        }

        [HttpGet("notes")]
        public async Task<IActionResult> ListNotes()
        {
            var list = await _notesRepository.ListNotesAsync(50);
            return Ok(list);
        }
    }

    public class GenerateSoapRequest
    {
        public string Transcript { get; set; } = string.Empty;
        public List<string>? KnowledgeIds { get; set; }
    }

    public class SaveNoteRequest
    {
        private SoapNote? note;

        public SoapNote? Note { get => note; set => note = value; }
        public string? Transcript { get; set; }
    }
}
