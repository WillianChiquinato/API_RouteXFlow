using API_RouteXFlow.Responses;

namespace API_RouteXFlow.Interfaces.Services;

public interface IStorageService
{
    Task<CustomResponse<string>> UploadFileAsync(Stream fileStream, string fileName, string contentType, string? folder = null);
    Task<CustomResponse<Stream>> DownloadFileAsync(string fileKey);
    Task<CustomResponse<bool>> DeleteFileAsync(string fileKey);
    Task<CustomResponse<string>> GetPresignedUrlAsync(string fileKey, int expirationMinutes = 60);
}
