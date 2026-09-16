using API_RouteXFlow.Domain.Data.Entities;
using API_RouteXFlow.Interfaces.Repository;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

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
        return await _dbContext.Users
            .AsNoTracking()
            .Where(x => x.Id == userId)
            .FirstOrDefaultAsync();
    }

    public async Task<User?> GetUserByLoginAsync(string email, string passwordHash)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Where(x => x.Email == email && x.PasswordHash == passwordHash)
            .FirstOrDefaultAsync();
    }
}