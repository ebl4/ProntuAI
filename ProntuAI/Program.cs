var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Register application services
builder.Services.AddSingleton<ProntuAI.Services.ITranscriptionService, ProntuAI.Services.MockTranscriptionService>();
builder.Services.AddSingleton<ProntuAI.Services.ILlmService, ProntuAI.Services.MockLlmService>();
builder.Services.AddSingleton<ProntuAI.Services.INotesRepository, ProntuAI.Services.FileNotesRepository>();
builder.Services.AddSingleton<ProntuAI.Services.IVectorStore, ProntuAI.Services.InMemoryVectorStore>();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Serve a minimal frontend for quick testing
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
