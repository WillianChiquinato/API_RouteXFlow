using API_RouteXFlow.Domain.Data.Entities;
using API_RouteXFlow.Interfaces.Repository;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace API_RouteXFlow.Repository;

public class AppRepository : IAppRepository
{
    private readonly AppDbContext _dbContext;

    public AppRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Apps>> GetApps()
    {
        return await _dbContext.Apps.AsNoTracking().ToListAsync();
    }

    public async Task<Apps> GetApp(int id)
    {
        return await _dbContext.Apps.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<bool> RegisterAppsVinculatedUser(List<int> apps, int userId)
    {
        foreach (var appId in apps)
        {
            var vinculated = new AppsVinculatedUser
            {
                AppId = appId,
                UserId = userId
            };
        
            _dbContext.Add(vinculated);
        }
    
        await _dbContext.SaveChangesAsync();
        return true;
    }
}