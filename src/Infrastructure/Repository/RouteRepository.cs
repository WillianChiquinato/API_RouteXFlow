using API_RouteXFlow.Domain.Data.Entities;
using API_RouteXFlow.Interfaces.Repository;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace API_RouteXFlow.Repository;

public class RouteRepository : IRouteRepository
{
    private readonly AppDbContext _dbContext;

    public RouteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<RoutePosition>> GetRoutesAsync(int userId, RouteFilterRequest filter, TypeApps? type)
    {
        var query = BaseQuery(userId);

        if (type.HasValue)
        {
            query = query.Where(route => route.Type == type.Value);
        }

        if (filter.WorkSessionId.HasValue)
        {
            query = query.Where(route => route.WorkSessionId == filter.WorkSessionId.Value);
        }

        if (filter.StartDate.HasValue)
        {
            query = query.Where(route => route.OriginGpsPosition!.Timestamp >= filter.StartDate.Value);
        }

        if (filter.EndDate.HasValue)
        {
            query = query.Where(route => route.OriginGpsPosition!.Timestamp <= filter.EndDate.Value);
        }

        return await query
            .OrderByDescending(route => route.OriginGpsPosition!.Timestamp)
            .ToListAsync();
    }

    public async Task<RoutePosition?> GetRouteByIdAsync(int userId, int id)
    {
        return await BaseQuery(userId).FirstOrDefaultAsync(route => route.Id == id);
    }

    private IQueryable<RoutePosition> BaseQuery(int userId)
    {
        return _dbContext.RoutePositions
            .AsNoTracking()
            .Include(route => route.OriginGpsPosition)
            .Include(route => route.DestinationGpsPosition)
            .Include(route => route.Stops)
                .ThenInclude(stop => stop.GpsPosition)
            .Include(route => route.Stops)
                .ThenInclude(stop => stop.StatusStop)
            .Where(route => route.WorkSession!.UserId == userId);
    }
}
