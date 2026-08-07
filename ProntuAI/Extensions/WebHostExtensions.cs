using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;

namespace ProntuAI.Extensions
{
    public static class WebHostExtensions
    {
        public static WebApplicationBuilder ConfigureRequestLimits(this WebApplicationBuilder builder)
        {
            // Configure Kestrel limits (500 MB)
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = 524_288_000; // 500 MB
            });

            // Configure form options for large multipart uploads
            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 524_288_000; // 500 MB
                options.ValueLengthLimit = int.MaxValue;
                options.MultipartHeadersLengthLimit = int.MaxValue;
            });

            return builder;
        }
    }
}
