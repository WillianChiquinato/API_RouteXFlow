using System.ComponentModel.DataAnnotations.Schema;

namespace API_RouteXFlow.Domain.Data.Entities;

[Table("audit_log")]
public class AuditLog : BaseEntity
{
    [Column("trace_id")]
    public string TraceId { get; set; } = string.Empty;

    [Column("user_id")]
    public int? UserId { get; set; }

    [Column("user_email")]
    public string? UserEmail { get; set; }

    [Column("ip_address")]
    public string? IpAddress { get; set; }

    [Column("user_agent")]
    public string? UserAgent { get; set; }

    [Column("http_method")]
    public string HttpMethod { get; set; } = string.Empty;

    [Column("path")]
    public string Path { get; set; } = string.Empty;

    [Column("query_string")]
    public string? QueryString { get; set; }

    [Column("endpoint")]
    public string? Endpoint { get; set; }

    [Column("status_code")]
    public int StatusCode { get; set; }

    [Column("duration_ms")]
    public long DurationMs { get; set; }

    [Column("request_body")]
    public string? RequestBody { get; set; }

    [Column("response_body")]
    public string? ResponseBody { get; set; }

    /// <summary>JSON array: [{ entity, action, key, before, after }]</summary>
    [Column("changes", TypeName = "jsonb")]
    public string? Changes { get; set; }

    [Column("exception_type")]
    public string? ExceptionType { get; set; }

    [Column("exception_message")]
    public string? ExceptionMessage { get; set; }

    [Column("exception_stack_trace")]
    public string? ExceptionStackTrace { get; set; }
}
