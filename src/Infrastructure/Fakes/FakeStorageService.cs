using Application.Common.Interfaces;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Fakes;

public class FakeStorageService : IStorageService
{
    public Task<string> UploadAsync(Stream stream, string objectKey, string contentType, CancellationToken ct = default)
    {
        return Task.FromResult($"https://fake-r2-storage.com/{objectKey}");
    }

    public Task<string> GetPresignedUrlAsync(string objectKey, int expiryHours = 168, CancellationToken ct = default)
    {
        return Task.FromResult($"https://fake-r2-storage.com/{objectKey}?token=presigned");
    }

    public Task<Stream> DownloadAsync(string objectKey, CancellationToken ct = default)
    {
        return Task.FromResult<Stream>(new MemoryStream());
    }

    public Task DeleteAsync(string objectKey, CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }
}
