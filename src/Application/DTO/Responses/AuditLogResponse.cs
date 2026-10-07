using System.Text.Json;

public class AuditLogListItemResponse
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string TraceId { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string HttpMethod { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public long DurationMs { get; set; }
    public bool HasChanges { get; set; }
    public string? ExceptionType { get; set; }
}

public class AuditLogDetailResponse : AuditLogListItemResponse
{
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? QueryString { get; set; }
    public string? Endpoint { get; set; }
    public string? RequestBody { get; set; }
    public string? ResponseBody { get; set; }
    public JsonElement? Changes { get; set; }
    public string? ExceptionMessage { get; set; }
    public string? ExceptionStackTrace { get; set; }
}
