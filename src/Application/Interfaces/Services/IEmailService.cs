using API_RouteXFlow.Domain.Data.Entities;
using API_RouteXFlow.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API_RouteXFlow.Interfaces.Services;

public interface IEmailService
{
    Task<CustomResponse<bool>> SendEmailAsync(EmailRequest request);
    Task<CustomResponse<bool>> SendEmailWithAttachmentAsync(EmailRequest request, IFormFile attachment);
    Task<CustomResponse<bool>> SendBulkEmailAsync(BulkEmailRequest request);
}