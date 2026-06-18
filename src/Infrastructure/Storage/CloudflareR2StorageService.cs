using Amazon.S3;
using Amazon.S3.Model;
using Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Storage;

public class CloudflareR2StorageService : IStorageService
{
    private readonly IAmazonS3 _s3;
    private readonly string _bucket;

    public CloudflareR2StorageService(IConfiguration config)
    {
        _bucket = config["Cloudflare:R2:BucketName"]
            ?? throw new ArgumentNullException("Cloudflare:R2:BucketName configuration is missing");
        
        var accessKeyId = config["Cloudflare:R2:AccessKeyId"]
            ?? throw new ArgumentNullException("Cloudflare:R2:AccessKeyId configuration is missing");
            
        var secretAccessKey = config["Cloudflare:R2:SecretAccessKey"]
            ?? throw new ArgumentNullException("Cloudflare:R2:SecretAccessKey configuration is missing");
            
        var serviceUrl = config["Cloudflare:R2:Endpoint"]
            ?? throw new ArgumentNullException("Cloudflare:R2:Endpoint configuration is missing");

        _s3 = new AmazonS3Client(
            accessKeyId,
            secretAccessKey,
            new AmazonS3Config
            {
                ServiceURL = serviceUrl,
                ForcePathStyle = true
            });
    }

    public async Task<string> UploadAsync(
        Stream stream, string objectKey, string contentType, CancellationToken ct = default)
    {
        var request = new PutObjectRequest
        {
            BucketName = _bucket,
            Key = objectKey,
            InputStream = stream,
            ContentType = contentType
        };

        await _s3.PutObjectAsync(request, ct);
        return objectKey;
    }

    public Task<string> GetPresignedUrlAsync(
        string objectKey, int expiryHours = 168, CancellationToken ct = default)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucket,
            Key = objectKey,
            Expires = DateTime.UtcNow.AddHours(expiryHours)
        };

        var url = _s3.GetPreSignedURL(request);
        return Task.FromResult(url);
    }

    public async Task<Stream> DownloadAsync(string objectKey, CancellationToken ct = default)
    {
        var res = await _s3.GetObjectAsync(_bucket, objectKey, ct);
        return res.ResponseStream;
    }

    public Task DeleteAsync(string objectKey, CancellationToken ct = default)
    {
        return _s3.DeleteObjectAsync(_bucket, objectKey, ct);
    }
}
