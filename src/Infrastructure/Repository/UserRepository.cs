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

    public async Task<List<User>> GetAllUsersAsync(string? email)
    {
        var query = _dbContext.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(email))
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            query = query.Where(x => x.Email == normalizedEmail);
        }

        return await query.ToListAsync();
    }

    public async Task<int> RegisterUserAsync(UserRegisterRequest userRegisterRequest)
    {
        var user = new User
        {
            Username = userRegisterRequest.Name.Trim(),
            Email = userRegisterRequest.Email.Trim().ToLowerInvariant(),
            Cpf = new string(userRegisterRequest.Cpf.Where(char.IsDigit).ToArray()),
            PhoneNumber = userRegisterRequest.PhoneNumber.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(userRegisterRequest.Password),
            RoleId = userRegisterRequest.RoleId,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);

        var affectedRows = await _dbContext.SaveChangesAsync();
        return affectedRows > 0 ? user.Id : 0;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Where(x => x.Email == email)
            .FirstOrDefaultAsync();
    }
}