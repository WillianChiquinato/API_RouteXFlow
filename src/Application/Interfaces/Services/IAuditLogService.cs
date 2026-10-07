using API_RouteXFlow.Responses;

namespace API_RouteXFlow.Interfaces.Services;

public interface IAuditLogService
{
    Task<CustomResponse<List<AuditLogListItemResponse>>> SearchAsync(int requesterId, AuditLogFilterRequest filter);
    Task<CustomResponse<AuditLogDetailResponse>> GetByIdAsync(int requesterId, int id);
}
