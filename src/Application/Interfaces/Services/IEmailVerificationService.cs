using API_RouteXFlow.Domain.Data.Entities;
using API_RouteXFlow.Responses;

namespace API_RouteXFlow.Interfaces.Services;

public interface IEmailVerificationService
{
    Task<CustomResponse<bool>> SendCodeAsync(User user);
    Task<CustomResponse<bool>> ResendCodeAsync(string email);
    Task<CustomResponse<User>> VerifyAsync(string email, string code);
}
