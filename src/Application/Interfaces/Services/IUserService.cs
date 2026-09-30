using API_RouteXFlow.Domain.Data.Entities;
using API_RouteXFlow.Responses;
using Application.DTO;
using Microsoft.AspNetCore.Identity.Data;

namespace API_RouteXFlow.Interfaces.Services;

public interface IUserService
{
    Task<CustomResponse<bool>> RegisterAsync(UserRegisterRequest userRegisterRequest);
    Task<CustomResponse<UserProfileDTO>> UpdateProfileAsync(int userId, UpdateProfileRequest request);
    Task<CustomResponse<UserPreferencesDTO>> UpdatePreferencesAsync(int userId, UpdatePreferencesRequest request);
    Task<CustomResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordRequest request);
}