using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Register application services
// Transcription: prefer Azure if configured, otherwise mock
var azureKey = Environment.GetEnvironmentVariable("AZURE_SPEECH_KEY");
var azureRegion = Environment.GetEnvironmentVariable("AZURE_SPEECH_REGION");
if (!string.IsNullOrEmpty(azureKey) && !string.IsNullOrEmpty(azureRegion))
{
    builder.Services.AddHttpClient<ProntuAI.Services.AzureTranscriptionService>();
    builder.Services.AddSingleton<ProntuAI.Services.ITranscriptionService, ProntuAI.Services.AzureTranscriptionService>();
}
else
{
    builder.Services.AddSingleton<ProntuAI.Services.ITranscriptionService, ProntuAI.Services.MockTranscriptionService>();
}

// LLM: use Gemini if configured, otherwise mock
var geminiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
var geminiEndpoint = Environment.GetEnvironmentVariable("GEMINI_ENDPOINT");
if (!string.IsNullOrEmpty(geminiKey) && !string.IsNullOrEmpty(geminiEndpoint))
{
    builder.Services.AddHttpClient<ProntuAI.Services.GeminiLlmService>();
    builder.Services.AddSingleton<ProntuAI.Services.ILlmService, ProntuAI.Services.GeminiLlmService>();
}
else
{
    builder.Services.AddSingleton<ProntuAI.Services.ILlmService, ProntuAI.Services.MockLlmService>();
}
// Configure EF Core (Postgres) if connection string present, otherwise fallback to file repo
var pgConn = Environment.GetEnvironmentVariable("PRONTUAI_DB_CONNECTION");
if (!string.IsNullOrEmpty(pgConn))
{
    builder.Services.AddDbContext<ProntuAI.Data.ApplicationDbContext>(opt => opt.UseNpgsql(pgConn));
    builder.Services.AddIdentityCore<Microsoft.AspNetCore.Identity.IdentityUser>().AddEntityFrameworkStores<ProntuAI.Data.ApplicationDbContext>();
    builder.Services.AddScoped<ProntuAI.Services.INotesRepository, ProntuAI.Services.EFNotesRepository>();
}
else
{
    builder.Services.AddSingleton<ProntuAI.Services.INotesRepository, ProntuAI.Services.FileNotesRepository>();
}
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

// Authentication / Identity
var jwtKey = builder.Configuration["Jwt:Key"] ?? Environment.GetEnvironmentVariable("PRONTUAI_JWT_KEY");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? Environment.GetEnvironmentVariable("PRONTUAI_JWT_ISSUER");
if (!string.IsNullOrEmpty(jwtKey) && !string.IsNullOrEmpty(jwtIssuer))
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.SaveToken = true;
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtIssuer,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtKey))
        };
    });

    app.UseAuthentication();
    app.UseAuthorization();
}

// Serve a minimal frontend for quick testing
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
