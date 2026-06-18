using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.AI;
using Infrastructure.Storage;
using Infrastructure.Video;
using Application.Common.Interfaces;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
                   .UseSnakeCaseNamingConvention());

        // Repositories
        services.AddScoped<IVideoJobRepository, VideoJobRepository>();
        services.AddScoped<IModelImageRepository, ModelImageRepository>();

        // AI & Scraper Services
        services.AddScoped<IProductScraperService, ShopeeProductScraper>();
        services.AddScoped<IProductCategoryDetector, OpenAiProductCategoryDetector>();
        services.AddScoped<IScriptGeneratorService, OpenAiScriptGeneratorService>();
        
        // Http Clients for external APIs
        services.AddHttpClient<IVoiceSynthesisService, ElevenLabsVoiceSynthesisService>();
        services.AddHttpClient<IAvatarVideoService, HeyGenAvatarVideoService>();
        services.AddHttpClient<IKlingService, KlingService>();

        // Storage & Video Services
        services.AddSingleton<IStorageService, CloudflareR2StorageService>();
        services.AddScoped<IVideoComposerService, FfmpegVideoComposerService>();

        return services;
    }
}
