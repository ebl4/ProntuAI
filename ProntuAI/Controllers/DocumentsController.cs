using Microsoft.AspNetCore.Mvc;
using ProntuAI.Services;

namespace ProntuAI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly IVectorStore _vectorStore;

        public DocumentsController(IVectorStore vectorStore)
        {
            _vectorStore = vectorStore;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] IFormFile file)
        {
            if (file == null) return BadRequest("Arquivo não enviado.");

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            ms.Position = 0;

            string content;
            if (file.ContentType.StartsWith("text/"))
            {
                using var reader = new StreamReader(ms);
                content = await reader.ReadToEndAsync();
            }
            else
            {
                // Try to parse PDF content when possible
                try
                {
                    ms.Position = 0;
                    using var pdfDoc = UglyToad.PdfPig.PdfDocument.Open(ms);
                    var txt = new System.Text.StringBuilder();
                    foreach (var page in pdfDoc.GetPages())
                    {
                        txt.AppendLine(page.Text);
                    }
                    content = txt.ToString();
                    if (string.IsNullOrWhiteSpace(content))
                        content = $"Documento: {file.FileName} (conteúdo não extraído).";
                }
                catch
                {
                    content = $"Documento: {file.FileName} (conteúdo não extraído).";
                }
            }

            var id = Guid.NewGuid().ToString();
            await _vectorStore.AddDocumentAsync(id, content);
            return Ok(new { id });
        }

        [HttpGet("query")]
        public async Task<IActionResult> Query([FromQuery] string q, [FromQuery] int topK = 3)
        {
            var results = await _vectorStore.QueryAsync(q, topK);
            return Ok(results);
        }
    }
}
