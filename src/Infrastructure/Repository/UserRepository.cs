using API_RouteXFlow.Domain.Data.Entities;
using API_RouteXFlow.Interfaces.Repository;
using Infrastructure.Persistence.Context;

namespace API_RouteXFlow.Repository;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _dbContext;

    public UserRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return null;
    }

    public async Task<User?> GetUserByLoginAsync(string username, string password)
    {
        return null;
    }
}