using API_RouteXFlow.Domain.Data.Entities;
using API_RouteXFlow.Interfaces.Repository;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace API_RouteXFlow.Repository;

public class AuditLogRepository : IAuditLogRepository
{
    private const int SuperAdminRoleId = 1;

    private readonly AppDbContext _dbContext;

    public AuditLogRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(List<AuditLogListItemResponse> Items, int Total)> SearchAsync(AuditLogFilterRequest f)
    {
        var query = _dbContext.AuditLogs.AsNoTracking().AsQueryable();

        if (f.UserId.HasValue) query = query.Where(a => a.UserId == f.UserId);
        if (!string.IsNullOrWhiteSpace(f.HttpMethod)) query = query.Where(a => a.HttpMethod == f.HttpMethod.ToUpper());
        if (!string.IsNullOrWhiteSpace(f.Path)) query = query.Where(a => EF.Functions.ILike(a.Path, $"%{f.Path}%"));
        if (f.StatusCode.HasValue) query = query.Where(a => a.StatusCode == f.StatusCode);
        if (f.OnlyErrors == true) query = query.Where(a => a.StatusCode >= 400 || a.ExceptionType != null);
        if (f.OnlyWithChanges == true) query = query.Where(a => a.Changes != null);
        if (!string.IsNullOrWhiteSpace(f.TraceId)) query = query.Where(a => a.TraceId == f.TraceId);
        if (f.From.HasValue) query = query.Where(a => a.CreatedAt >= f.From.Value.ToUniversalTime());
        if (f.To.HasValue) query = query.Where(a => a.CreatedAt <= f.To.Value.ToUniversalTime());

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((f.Page - 1) * f.PageSize)
            .Take(f.PageSize)
            .Select(a => new AuditLogListItemResponse
            {
                Id = a.Id,
                CreatedAt = a.CreatedAt,
                TraceId = a.TraceId,
                UserId = a.UserId,
                UserEmail = a.UserEmail,
                HttpMethod = a.HttpMethod,
                Path = a.Path,
                StatusCode = a.StatusCode,
                DurationMs = a.DurationMs,
                HasChanges = a.Changes != null,
                ExceptionType = a.ExceptionType
            })
            .ToListAsync();

        return (items, total);
    }

    public Task<AuditLog?> GetByIdAsync(int id) =>
        _dbContext.AuditLogs.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);

    public Task<bool> IsSuperAdminAsync(int userId) =>
        _dbContext.Users.AsNoTracking().AnyAsync(u => u.Id == userId && u.RoleId == SuperAdminRoleId);
}
