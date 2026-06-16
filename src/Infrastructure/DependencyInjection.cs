using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Fakes;
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

        // Mock/Fake Services for Phase 2 API Verification
        services.AddScoped<IProductScraperService, FakeProductScraperService>();
        services.AddScoped<IProductCategoryDetector, FakeProductCategoryDetector>();
        services.AddScoped<IScriptGeneratorService, FakeScriptGeneratorService>();
        services.AddScoped<IVoiceSynthesisService, FakeVoiceSynthesisService>();
        services.AddScoped<IAvatarVideoService, FakeAvatarVideoService>();
        services.AddScoped<IKlingService, FakeKlingService>();
        services.AddScoped<IVideoComposerService, FakeVideoComposerService>();
        services.AddScoped<IStorageService, FakeStorageService>();

        return services;
    }
}
