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
            .Include(x => x.Role)
            .Include(x => x.Preferences)
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
            EmailVerified = false,
            CreatedAt = DateTime.UtcNow,
            Preferences = { new Preferences() }
        };

        _dbContext.Users.Add(user);

        var affectedRows = await _dbContext.SaveChangesAsync();
        return affectedRows > 0 ? user.Id : 0;
    }

    public async Task<Preferences?> GetPreferencesUserByIdAsync(int userId)
    {
        return await _dbContext.Preferences
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> UpdatePreferenceAsync(Preferences preferencesUser)
    {
        // Usuários anteriores à tabela de preferências ainda não têm linha: cria na primeira gravação.
        if (preferencesUser.Id == 0)
            _dbContext.Preferences.Add(preferencesUser);
        else
            _dbContext.Preferences.Update(preferencesUser);

        return await _dbContext.SaveChangesAsync() > 0;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Where(x => x.Email == email)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> UpdateUserAsync(User user)
    {
        _dbContext.Users.Update(user);
        return await _dbContext.SaveChangesAsync() > 0;
    }

    public async Task<bool> SavePasswordResetTokenAsync(int userId, string resetToken)
    {
        if (string.IsNullOrWhiteSpace(resetToken))
            return false;

        _dbContext.EmailCodes.Add(new EmailCode
        {
            UserId = userId,
            Code = resetToken,
            Purpose = EmailCodePurpose.PasswordReset,
            ExpirationTime = DateTime.UtcNow.AddMinutes(15),
            CreatedAt = DateTime.UtcNow
        });

        return await _dbContext.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateUserPasswordAsync(User user, string newPassword)
    {
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        _dbContext.Users.Update(user);

        return await _dbContext.SaveChangesAsync() > 0;
    }

    public async Task<User?> GetUserByResetTokenAsync(string resetToken)
    {
        var emailCode = await _dbContext.EmailCodes
            .AsNoTracking()
            .Where(x => x.Purpose == EmailCodePurpose.PasswordReset && x.Code == resetToken && x.ExpirationTime > DateTime.UtcNow)
            .FirstOrDefaultAsync();

        if (emailCode == null)
            return null;

        return await _dbContext.Users
            .AsNoTracking()
            .Where(x => x.Id == emailCode.UserId)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> InvalidateResetTokenAsync(int userId)
    {
        var emailCode = await _dbContext.EmailCodes
            .Where(x => x.UserId == userId && x.Purpose == EmailCodePurpose.PasswordReset)
            .FirstOrDefaultAsync();

        if (emailCode == null)
            return false;

        _dbContext.EmailCodes.Remove(emailCode);
        return await _dbContext.SaveChangesAsync() > 0;
    }

    public async Task<bool> SaveEmailVerificationCodeAsync(int userId, string code)
    {
        // Só vale o último código enviado: os anteriores são descartados.
        var previous = await _dbContext.EmailCodes
            .Where(x => x.UserId == userId && x.Purpose == EmailCodePurpose.EmailVerification)
            .ToListAsync();
        _dbContext.EmailCodes.RemoveRange(previous);

        _dbContext.EmailCodes.Add(new EmailCode
        {
            UserId = userId,
            Code = code,
            Purpose = EmailCodePurpose.EmailVerification,
            ExpirationTime = DateTime.UtcNow.AddMinutes(15),
            CreatedAt = DateTime.UtcNow
        });

        return await _dbContext.SaveChangesAsync() > 0;
    }

    public async Task<EmailCode?> GetEmailVerificationCodeAsync(int userId)
    {
        return await _dbContext.EmailCodes
            .Where(x => x.UserId == userId && x.Purpose == EmailCodePurpose.EmailVerification)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> RemoveEmailCodeAsync(EmailCode emailCode)
    {
        _dbContext.EmailCodes.Remove(emailCode);
        return await _dbContext.SaveChangesAsync() > 0;
    }

    public async Task<bool> RegisterEmailCodeAttemptAsync(EmailCode emailCode)
    {
        emailCode.Attempts++;
        _dbContext.EmailCodes.Update(emailCode);
        return await _dbContext.SaveChangesAsync() > 0;
    }

    public async Task<bool> MarkEmailVerifiedAsync(int userId)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return false;

        user.EmailVerified = true;
        return await _dbContext.SaveChangesAsync() > 0;
    }

    public async Task<ProfilesExternal?> GetProfileUserByIdAsync(int userId)
    {
        return await _dbContext.ProfilesExternals
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .FirstOrDefaultAsync();
    }

}
