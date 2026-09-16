using API_RouteXFlow.Domain.Data.Entities;
using API_RouteXFlow.Responses;
using Microsoft.AspNetCore.Identity.Data;

namespace API_RouteXFlow.Interfaces.Services;

public interface IAuthService
{
    Task<CustomResponse<string>> LoginAsync(LoginRequest request);
    Task<CustomResponse<string>> RefreshAsync(string token);
    Task<CustomResponse<User>> SearchUserByIdAsync(int userId);
}