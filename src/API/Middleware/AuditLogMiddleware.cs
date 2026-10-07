using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using API_RouteXFlow.Domain.Data.Entities;
using Infrastructure.Audit;
using Infrastructure.Persistence.Context;

namespace API_RouteXFlow.Middleware;

public class AuditLogMiddleware
{
    private const int MaxBodyLength = 32 * 1024;
    private const int MaxStackTraceLength = 8 * 1024;

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };

    private readonly RequestDelegate _next;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuditLogMiddleware> _logger;

    public AuditLogMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory, ILogger<AuditLogMiddleware> logger)
    {
        _next = next;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (ShouldSkip(context))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var captureResponse = !IsStreamingPath(context.Request.Path);

        var requestBody = await ReadRequestBodyAsync(context.Request);

        var originalBody = context.Response.Body;
        using var responseBuffer = new MemoryStream();
        if (captureResponse) context.Response.Body = responseBuffer;

        Exception? exception = null;
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            exception = ex;
            throw;
        }
        finally
        {
            stopwatch.Stop();

            string? responseBody = null;
            if (captureResponse)
            {
                if (IsTextual(context.Response.ContentType))
                {
                    responseBody = Encoding.UTF8.GetString(responseBuffer.GetBuffer(), 0, (int)Math.Min(responseBuffer.Length, int.MaxValue));
                }

                responseBuffer.Position = 0;
                context.Response.Body = originalBody;
                if (exception is null) await responseBuffer.CopyToAsync(originalBody, context.RequestAborted);
            }

            await SaveAsync(context, requestBody, responseBody, exception, stopwatch.ElapsedMilliseconds);
        }
    }

    private async Task SaveAsync(HttpContext context, string? requestBody, string? responseBody, Exception? exception, long durationMs)
    {
        try
        {
            var trail = context.RequestServices.GetRequiredService<AuditTrail>();
            var changes = trail.Snapshot();

            int? userId = int.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

            var log = new AuditLog
            {
                TraceId = context.TraceIdentifier,
                UserId = userId,
                UserEmail = context.User.FindFirstValue(ClaimTypes.Email),
                IpAddress = ResolveIp(context),
                UserAgent = Truncate(context.Request.Headers.UserAgent.ToString(), 512),
                HttpMethod = context.Request.Method,
                Path = context.Request.Path.Value ?? string.Empty,
                QueryString = SanitizeQuery(context.Request.Query),
                Endpoint = context.GetEndpoint()?.DisplayName,
                StatusCode = exception is null ? context.Response.StatusCode : StatusCodes.Status500InternalServerError,
                DurationMs = durationMs,
                RequestBody = AuditRedactor.SanitizeJson(requestBody, MaxBodyLength),
                ResponseBody = AuditRedactor.SanitizeJson(responseBody, MaxBodyLength),
                Changes = changes.Count > 0 ? JsonSerializer.Serialize(changes, JsonOptions) : null,
                ExceptionType = exception?.GetType().FullName,
                ExceptionMessage = exception?.Message,
                ExceptionStackTrace = Truncate(exception?.ToString(), MaxStackTraceLength)
            };

            // Scope próprio: o log não deve depender (nem ser revertido junto) do DbContext da requisição.
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.AuditLogs.Add(log);
            await db.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            // Auditoria nunca pode derrubar a requisição.
            _logger.LogError(ex, "Falha ao gravar audit_log para {TraceId}", context.TraceIdentifier);
        }
    }

    private static async Task<string?> ReadRequestBodyAsync(HttpRequest request)
    {
        if (request.ContentLength is 0 || !IsTextual(request.ContentType)) return null;

        request.EnableBuffering();
        using var reader = new StreamReader(request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;
        return body;
    }

    private static bool ShouldSkip(HttpContext context)
    {
        var path = context.Request.Path;
        return context.WebSockets.IsWebSocketRequest
            || HttpMethods.IsOptions(context.Request.Method)
            || path == "/"
            || path.StartsWithSegments("/api/audit", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase);
    }

    // Downloads de arquivo: não faz buffer da resposta em memória.
    private static bool IsStreamingPath(PathString path) =>
        path.StartsWithSegments("/api/file/download", StringComparison.OrdinalIgnoreCase);

    private static bool IsTextual(string? contentType) =>
        contentType is not null &&
        (contentType.Contains("json", StringComparison.OrdinalIgnoreCase)
         || contentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase)
         || contentType.Contains("x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase));

    private static string? SanitizeQuery(IQueryCollection query)
    {
        if (query.Count == 0) return null;
        return string.Join("&", query.Select(q =>
            $"{q.Key}={(AuditRedactor.IsSensitive(q.Key) ? "***" : q.Value.ToString())}"));
    }

    private static string? ResolveIp(HttpContext context)
    {
        var forwarded = context.Request.Headers["X-Forwarded-For"].ToString();
        return string.IsNullOrWhiteSpace(forwarded)
            ? context.Connection.RemoteIpAddress?.ToString()
            : forwarded.Split(',')[0].Trim();
    }

    private static string? Truncate(string? value, int max) =>
        value is null || value.Length <= max ? value : value[..max];
}
