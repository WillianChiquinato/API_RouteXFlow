using API_RouteXFlow.Domain.Data.Entities;

namespace API_RouteXFlow.Interfaces.Repository;

public interface IAuditLogRepository
{
    Task<(List<AuditLogListItemResponse> Items, int Total)> SearchAsync(AuditLogFilterRequest filter);
    Task<AuditLog?> GetByIdAsync(int id);
    Task<bool> IsSuperAdminAsync(int userId);
}
