public class AuditLogFilterRequest
{
    public int? UserId { get; set; }
    public string? HttpMethod { get; set; }
    public string? Path { get; set; }
    public int? StatusCode { get; set; }
    public bool? OnlyErrors { get; set; }
    public bool? OnlyWithChanges { get; set; }
    public string? TraceId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
