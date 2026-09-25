using API_RouteXFlow.Domain.Data.Entities;
using API_RouteXFlow.Responses;

namespace API_RouteXFlow.Interfaces.Services;

public interface IContainerService
{
    Task<CustomResponse<List<ContainerToDevicesDTO>>> GetContainers();
    Task<CustomResponse<bool>> RegisterAsync(ContainerRegisterRequest containerRegisterRequest, int userId);
    Task<CustomResponse<bool>> EditAsync(ContainerEditRequest containerEditRequest, int userId);
}
