using API_RouteXFlow.Responses;

namespace API_RouteXFlow.Interfaces.Services;

public interface IAuthService
{
    Task<CustomResponse<string>> LoginAsync(LoginRequest request);
    Task<bool> ValidateTokenAsync(string token);
    Task LogoutAsync();
}