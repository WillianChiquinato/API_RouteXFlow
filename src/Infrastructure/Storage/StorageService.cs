using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using API_RouteXFlow.Interfaces.Services;
using API_RouteXFlow.Responses;
using Microsoft.Extensions.Logging;

namespace API_RouteXFlow.Services;

public class StorageService : IStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<StorageService> _logger;
    private readonly string _bucketName;
    private readonly Protocol _presignedUrlProtocol;

    public StorageService(IAmazonS3 s3Client, ILogger<StorageService> logger)
    {
        _s3Client = s3Client;
        _logger = logger;
        _bucketName = Environment.GetEnvironmentVariable("STORAGE_BUCKET_NAME") ?? "routexflow";

        var storageEndpoint = Environment.GetEnvironmentVariable("STORAGE_ENDPOINT") ?? "http://localhost:8333";
        _presignedUrlProtocol = storageEndpoint.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            ? Protocol.HTTP
            : Protocol.HTTPS;
    }

    public async Task<CustomResponse<string>> UploadFileAsync(Stream fileStream, string fileName, string contentType, string? folder = null)
    {
        try
        {
            var fileKey = BuildFileKey(fileName, folder);

            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = fileKey,
                InputStream = fileStream,
                ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
                AutoCloseStream = true
            };

            await _s3Client.PutObjectAsync(request);

            return CustomResponse<string>.SuccessTrade(fileKey);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Erro ao enviar arquivo {FileName} para o storage", fileName);
            return CustomResponse<string>.Fail("Não foi possível enviar o arquivo.");
        }
    }

    public async Task<CustomResponse<Stream>> DownloadFileAsync(string fileKey)
    {
        try
        {
            var response = await _s3Client.GetObjectAsync(_bucketName, fileKey);
            return CustomResponse<Stream>.SuccessTrade(response.ResponseStream);
        }
        catch (AmazonS3Exception e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            return CustomResponse<Stream>.Fail("Arquivo não encontrado.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Erro ao baixar arquivo {FileKey} do storage", fileKey);
            return CustomResponse<Stream>.Fail("Não foi possível baixar o arquivo.");
        }
    }

    public async Task<CustomResponse<bool>> DeleteFileAsync(string fileKey)
    {
        try
        {
            await _s3Client.DeleteObjectAsync(_bucketName, fileKey);
            return CustomResponse<bool>.SuccessTrade(true);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Erro ao excluir arquivo {FileKey} do storage", fileKey);
            return CustomResponse<bool>.Fail("Não foi possível excluir o arquivo.");
        }
    }

    public async Task<CustomResponse<string>> GetPresignedUrlAsync(string fileKey, int expirationMinutes = 60)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = fileKey,
                Verb = HttpVerb.GET,
                Protocol = _presignedUrlProtocol,
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes)
            };

            var url = await _s3Client.GetPreSignedURLAsync(request);
            return CustomResponse<string>.SuccessTrade(url);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Erro ao gerar URL do arquivo {FileKey}", fileKey);
            return CustomResponse<string>.Fail("Não foi possível gerar a URL do arquivo.");
        }
    }

    private static string BuildFileKey(string fileName, string? folder)
    {
        var safeFileName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
        return string.IsNullOrWhiteSpace(folder) ? safeFileName : $"{folder.Trim('/')}/{safeFileName}";
    }
}
