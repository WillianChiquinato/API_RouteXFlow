using API_RouteXFlow.Domain.Data.Entities;

namespace API_RouteXFlow.Interfaces.Repository;

public interface IRouteRepository
{
    Task<List<RoutePosition>> GetRoutesAsync(int userId, RouteFilterRequest filter, TypeApps? type);
    Task<RoutePosition?> GetRouteByIdAsync(int userId, int id);
}
