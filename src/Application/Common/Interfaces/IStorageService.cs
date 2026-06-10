using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Interfaces
{
    public interface IStorageService
    {
        Task<string> UploadAsync(Stream stream, string objectKey, string contentType, CancellationToken ct = default);
        Task<string> GetPresignedUrlAsync(string objectKey, CancellationToken ct = default);
        Task DeleteAsync(string objectKey, CancellationToken ct = default);
    }
}
