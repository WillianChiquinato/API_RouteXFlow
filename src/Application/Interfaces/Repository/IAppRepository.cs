using API_RouteXFlow.Domain.Data.Entities;

namespace API_RouteXFlow.Interfaces.Repository;

public interface IAppRepository
{
    Task<List<Apps>> GetApps();
    Task<Apps> GetApp(int id);
    Task<bool> RegisterAppsVinculatedUser(List<int> apps, int userId);
}