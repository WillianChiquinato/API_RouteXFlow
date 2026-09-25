using API_RouteXFlow.Domain.Data.Entities;
using API_RouteXFlow.Responses;
using Microsoft.AspNetCore.Mvc;

namespace API_RouteXFlow.Interfaces.Services;

public interface IDeviceService
{
    Task<CustomResponse<List<Device>>> GetDevices();
    Task<CustomResponse<bool>> RegisterAsync(DeviceRegisterRequest request, int userId);
    Task<CustomResponse<bool>> EditAsync(DeviceEditRequest request, int userId);
    Task<CustomResponse<bool>> DeleteAsync(int id, int userId);
}