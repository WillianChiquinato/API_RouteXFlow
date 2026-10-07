using System.Text.Json;
using API_RouteXFlow.Interfaces.Repository;
using API_RouteXFlow.Interfaces.Services;
using API_RouteXFlow.Responses;

namespace API_RouteXFlow.Services;

public class AuditLogService : IAuditLogService
{
    private const string Forbidden = "Acesso restrito ao perfil Super-Admin.";

    private readonly IAuditLogRepository _repository;

    public AuditLogService(IAuditLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<CustomResponse<List<AuditLogListItemResponse>>> SearchAsync(int requesterId, AuditLogFilterRequest filter)
    {
        if (!await _repository.IsSuperAdminAsync(requesterId))
            return CustomResponse<List<AuditLogListItemResponse>>.Fail(Forbidden);

        filter.Page = Math.Max(filter.Page, 1);
        filter.PageSize = Math.Clamp(filter.PageSize, 1, 100);

        var (items, total) = await _repository.SearchAsync(filter);
        return CustomResponse<List<AuditLogListItemResponse>>.SuccessTrade(items, total);
    }

    public async Task<CustomResponse<AuditLogDetailResponse>> GetByIdAsync(int requesterId, int id)
    {
        if (!await _repository.IsSuperAdminAsync(requesterId))
            return CustomResponse<AuditLogDetailResponse>.Fail(Forbidden);

        var log = await _repository.GetByIdAsync(id);
        if (log is null)
            return CustomResponse<AuditLogDetailResponse>.Fail("Registro de auditoria não encontrado.");

        var detail = new AuditLogDetailResponse
        {
            Id = log.Id,
            CreatedAt = log.CreatedAt,
            TraceId = log.TraceId,
            UserId = log.UserId,
            UserEmail = log.UserEmail,
            HttpMethod = log.HttpMethod,
            Path = log.Path,
            StatusCode = log.StatusCode,
            DurationMs = log.DurationMs,
            HasChanges = log.Changes != null,
            ExceptionType = log.ExceptionType,
            IpAddress = log.IpAddress,
            UserAgent = log.UserAgent,
            QueryString = log.QueryString,
            Endpoint = log.Endpoint,
            RequestBody = log.RequestBody,
            ResponseBody = log.ResponseBody,
            Changes = log.Changes is null ? null : JsonDocument.Parse(log.Changes).RootElement.Clone(),
            ExceptionMessage = log.ExceptionMessage,
            ExceptionStackTrace = log.ExceptionStackTrace
        };

        return CustomResponse<AuditLogDetailResponse>.SuccessTrade(detail);
    }
}
