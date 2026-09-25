using API_RouteXFlow.Domain.Data.Entities;

namespace API_RouteXFlow.Interfaces.Repository;

public interface IUserRepository
{
    Task<User?> GetUserByIdAsync(int userId);
    Task<List<User>> GetAllUsersAsync(string? email);
    Task<int> RegisterUserAsync(UserRegisterRequest userRegisterRequest);
    Task<User?> GetUserByEmailAsync(string email);
}