using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Application.Common.Interfaces;
using Infrastructure;
using System.Collections.Generic;

namespace Integration.Tests;

public class DependencyResolutionTests
{
    [Fact]
    public void AddInfrastructure_ShouldResolveAllServices()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"ConnectionStrings:DefaultConnection", "Host=localhost;Database=test;Username=postgres;Password=postgres"},
            {"OpenAI:ApiKey", "fake-openai-key"},
            {"ElevenLabs:ApiKey", "fake-elevenlabs-key"},
            {"HeyGen:ApiKey", "fake-heygen-key"},
            {"Kling:AccessKey", "fake-kling-access-key"},
            {"Kling:SecretKey", "fake-kling-secret-key"},
            {"Cloudflare:R2:Endpoint", "https://fake-account-id.r2.cloudflarestorage.com"},
            {"Cloudflare:R2:AccessKeyId", "fake-r2-access-key"},
            {"Cloudflare:R2:SecretAccessKey", "fake-r2-secret-key"},
            {"Cloudflare:R2:BucketName", "fake-bucket"},
            {"Cloudflare:R2:PublicUrl", "https://files.fake.com"},
            {"FFmpeg:Path", "ffmpeg"}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        // Act
        services.AddInfrastructure(configuration);
        var provider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(provider.GetService<IProductScraperService>());
        Assert.NotNull(provider.GetService<IProductCategoryDetector>());
        Assert.NotNull(provider.GetService<IScriptGeneratorService>());
        Assert.NotNull(provider.GetService<IVoiceSynthesisService>());
        Assert.NotNull(provider.GetService<IAvatarVideoService>());
        Assert.NotNull(provider.GetService<IKlingService>());
        Assert.NotNull(provider.GetService<IStorageService>());
        Assert.NotNull(provider.GetService<IVideoComposerService>());
    }
}
