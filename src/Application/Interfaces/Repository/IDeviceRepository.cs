using API_RouteXFlow.Domain.Data.Entities;

namespace API_RouteXFlow.Interfaces.Repository;

public interface IDeviceRepository
{
    Task<List<Device>> GetDevicesAsync();
    Task<bool> RegisterDeviceAsync(DeviceRegisterRequest request);
    Task<int?> GetOwnerUserIdByDeviceIdAsync(int deviceId);
    Task<bool> UpdateDeviceAsync(DeviceEditRequest request);
    Task<bool> DeleteDeviceAsync(int id);
}