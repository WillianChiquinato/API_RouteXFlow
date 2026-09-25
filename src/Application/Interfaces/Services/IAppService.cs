using API_RouteXFlow.Domain.Data.Entities;
using API_RouteXFlow.Responses;

namespace API_RouteXFlow.Interfaces.Services;

public interface IAppService
{
    Task<CustomResponse<List<Apps>>> GetAllAsync();
    Task<CustomResponse<Apps>> GetByIdAsync(int id);
}