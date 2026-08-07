using Microsoft.EntityFrameworkCore;
using ProntuAI.Services;

namespace ProntuAI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddProntuAiServices(this IServiceCollection services, IConfiguration config)
        {
            // Transcription
            var azureKey = Environment.GetEnvironmentVariable("AZURE_SPEECH_KEY");
            var azureRegion = Environment.GetEnvironmentVariable("AZURE_SPEECH_REGION");
            if (!string.IsNullOrEmpty(azureKey) && !string.IsNullOrEmpty(azureRegion))
            {
                services.AddHttpClient<AzureTranscriptionService>();
                services.AddSingleton<ITranscriptionService, AzureTranscriptionService>();
            }
            else
            {
                services.AddSingleton<ITranscriptionService, MockTranscriptionService>();
            }

            // LLM
            var geminiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
            var geminiEndpoint = Environment.GetEnvironmentVariable("GEMINI_ENDPOINT");
            if (!string.IsNullOrEmpty(geminiKey) && !string.IsNullOrEmpty(geminiEndpoint))
            {
                services.AddHttpClient<GeminiLlmService>();
                services.AddSingleton<ILlmService, GeminiLlmService>();
            }
            else
            {
                services.AddSingleton<ILlmService, MockLlmService>();
            }

            // Repository / Persistence
            var pgConn = Environment.GetEnvironmentVariable("PRONTUAI_DB_CONNECTION");
            if (!string.IsNullOrEmpty(pgConn))
            {
                services.AddDbContext<ProntuAI.Data.ApplicationDbContext>(opt => opt.UseNpgsql(pgConn));
                services.AddIdentityCore<Microsoft.AspNetCore.Identity.IdentityUser>().AddEntityFrameworkStores<ProntuAI.Data.ApplicationDbContext>();
                services.AddScoped<INotesRepository, ProntuAI.Infrastructure.Repositories.EFNotesRepository>();
            }
            else
            {
                services.AddSingleton<INotesRepository, ProntuAI.Infrastructure.Repositories.FileNotesRepository>();
            }

            // Vector store
            services.AddSingleton<IVectorStore, InMemoryVectorStore>();

            return services;
        }
    }
}
