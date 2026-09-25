using API_RouteXFlow.Domain.Data.Entities;
using API_RouteXFlow.Responses;
using Microsoft.AspNetCore.Identity.Data;

namespace API_RouteXFlow.Interfaces.Services;

public interface IUserService
{
    Task<CustomResponse<bool>> RegisterAsync(UserRegisterRequest userRegisterRequest);
}