using API_RouteXFlow.Domain.Data.Entities;
using API_RouteXFlow.Responses;

namespace API_RouteXFlow.Interfaces.Services;

public interface IRoleService
{
    Task<CustomResponse<List<Role>>> GetRoles();
}