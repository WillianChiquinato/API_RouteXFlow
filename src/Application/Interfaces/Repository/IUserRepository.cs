using API_RouteXFlow.Domain.Data.Entities;

namespace API_RouteXFlow.Interfaces.Repository;

public interface IUserRepository
{
    Task<User?> GetUserByIdAsync(int userId);
    Task<List<User>> GetAllUsersAsync(string? email);
    Task<int> RegisterUserAsync(UserRegisterRequest userRegisterRequest);
    Task<Preferences?> GetPreferencesUserByIdAsync(int userId);
    Task<bool> UpdatePreferenceAsync(Preferences preferencesUser);
    Task<User?> GetUserByEmailAsync(string email);
    Task<bool> UpdateUserAsync(User user);
    Task<bool> SavePasswordResetTokenAsync(int userId, string resetToken);
    Task<bool> UpdateUserPasswordAsync(User user, string newPassword);
    Task<User?> GetUserByResetTokenAsync(string resetToken);
    Task<bool> InvalidateResetTokenAsync(int userId);
    Task<bool> SaveEmailVerificationCodeAsync(int userId, string code);
    Task<EmailCode?> GetEmailVerificationCodeAsync(int userId);
    Task<bool> RemoveEmailCodeAsync(EmailCode emailCode);
    Task<bool> RegisterEmailCodeAttemptAsync(EmailCode emailCode);
    Task<bool> MarkEmailVerifiedAsync(int userId);
}