using API_RouteXFlow.Interfaces.Services;
using API_RouteXFlow.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_RouteXFlow.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FileController : ControllerBase
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB

    private readonly IStorageService _storageService;

    public FileController(IStorageService storageService)
    {
        _storageService = storageService;
    }

    [HttpPost]
    [Route("upload")]
    [RequestSizeLimit(MaxFileSizeBytes)]
    public async Task<IActionResult> Upload(IFormFile? file, [FromQuery] string? folder = null)
    {
        if (file is null || file.Length == 0)
            return BadRequest(CustomResponse<string>.Fail("Nenhum arquivo enviado."));

        if (file.Length > MaxFileSizeBytes)
            return BadRequest(CustomResponse<string>.Fail("Arquivo excede o tamanho máximo permitido (10MB)."));

        await using var stream = file.OpenReadStream();
        var result = await _storageService.UploadFileAsync(stream, file.FileName, file.ContentType, folder);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet]
    [Route("download/{**fileKey}")]
    public async Task<IActionResult> Download(string fileKey)
    {
        var result = await _storageService.DownloadFileAsync(fileKey);

        if (!result.Success || result.Result is null)
            return NotFound(result);

        return File(result.Result, "application/octet-stream", Path.GetFileName(fileKey));
    }

    [HttpDelete]
    [Route("{**fileKey}")]
    public async Task<IActionResult> Delete(string fileKey)
    {
        var result = await _storageService.DeleteFileAsync(fileKey);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet]
    [Route("url/{**fileKey}")]
    public async Task<IActionResult> GetUrl(string fileKey, [FromQuery] int expirationMinutes = 60)
    {
        var result = await _storageService.GetPresignedUrlAsync(fileKey, expirationMinutes);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
