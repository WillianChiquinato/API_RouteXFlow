using API_RouteXFlow.Domain.Data.Entities;
using API_RouteXFlow.Responses;
using Microsoft.AspNetCore.Identity.Data;

namespace API_RouteXFlow.Interfaces.Services;

public interface IAuthService
{
    Task<CustomResponse<string>> LoginAsync(LoginRequest request);
    Task<CustomResponse<string>> RefreshAsync(string token);
    Task<CustomResponse<string>> VerifyEmailAsync(VerifyEmailRequest request);
    Task<CustomResponse<bool>> ResendVerificationAsync(ResendVerificationRequest request);
    Task<CustomResponse<User>> SearchUserByIdAsync(int userId);
    Task<CustomResponse<string>> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<CustomResponse<string>> ResetPasswordAsync(ResetPasswordRequest request);
    Task<CustomResponse<string>> ValidateResetTokenAsync(string token);
}