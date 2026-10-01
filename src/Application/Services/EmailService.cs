using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using API_RouteXFlow.Domain.Data.Entities;
using API_RouteXFlow.Interfaces.Repository;
using API_RouteXFlow.Interfaces.Services;
using API_RouteXFlow.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace API_RouteXFlow.Services;

public class EmailService : IEmailService
{
    private readonly HttpClient _httpClient;

    public EmailService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CustomResponse<bool>> SendEmailAsync(EmailRequest request)
    {
        var requestData = new
        {
            from = "onboarding@resend.dev",
            to = new[] { request.To },
            subject = request.Subject,
            html = request.Body
        };

        var jsonContent = JsonSerializer.Serialize(requestData);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Environment.GetEnvironmentVariable("EMAIL_API_KEY"));

        var response = await _httpClient.PostAsync(Environment.GetEnvironmentVariable("EMAIL_API_URL"), content);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("E-mail enviado com sucesso via Resend!");
            return new CustomResponse<bool>(true, new List<string>(), true);
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Erro ao enviar e-mail: {errorContent}");
            return new CustomResponse<bool>(false, new List<string> { "Erro ao enviar e-mail." }, false);
        }
    }

    public async Task<CustomResponse<bool>> SendEmailWithAttachmentAsync(EmailRequest request, IFormFile attachment)
    {
        var requestData = new
        {
            to = request.To,
            subject = request.Subject,
            html = request.Body,
            attachments = new[]
            {
                new
                {
                    filename = attachment.FileName,
                    content = Convert.ToBase64String(await ReadFileAsync(attachment))
                }
            }
        };

        var jsonContent = JsonSerializer.Serialize(requestData);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Environment.GetEnvironmentVariable("EMAIL_API_KEY"));

        var response = await _httpClient.PostAsync(Environment.GetEnvironmentVariable("EMAIL_API_URL"), content);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("E-mail com anexo enviado com sucesso via Resend!");
            return new CustomResponse<bool>(true, new List<string>(), true);
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Erro ao enviar e-mail com anexo: {errorContent}");
            return new CustomResponse<bool>(false, new List<string> { "Erro ao enviar e-mail com anexo." }, false);
        }
    }

    private async Task<byte[]> ReadFileAsync(IFormFile attachment)
    {
        using var memoryStream = new MemoryStream();
        await attachment.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

    public async Task<CustomResponse<bool>> SendBulkEmailAsync(BulkEmailRequest request)
    {
        var requestData = new
        {
            to = request.To,
            subject = request.Subject,
            html = request.Body
        };

        var jsonContent = JsonSerializer.Serialize(requestData);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Environment.GetEnvironmentVariable("EMAIL_API_KEY"));

        var response = await _httpClient.PostAsync(Environment.GetEnvironmentVariable("EMAIL_API_URL"), content);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("E-mails em massa enviados com sucesso via Resend!");
            return new CustomResponse<bool>(true, new List<string>(), true);
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Erro ao enviar e-mails em massa: {errorContent}");
            return new CustomResponse<bool>(false, new List<string> { "Erro ao enviar e-mails em massa." }, false);
        }
    }
}