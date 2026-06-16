namespace Application.Common.Interfaces;

using System.IO;
using System.Threading;
using System.Threading.Tasks;

public interface IStorageService
{
    Task<string> UploadAsync(Stream stream, string objectKey, string contentType, CancellationToken ct = default);
    Task<string> GetPresignedUrlAsync(string objectKey, int expiryHours = 168, CancellationToken ct = default);
    Task<Stream> DownloadAsync(string objectKey, CancellationToken ct = default);
    Task DeleteAsync(string objectKey, CancellationToken ct = default);
}
