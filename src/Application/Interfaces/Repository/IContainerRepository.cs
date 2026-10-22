using API_RouteXFlow.Domain.Data.Entities;

namespace API_RouteXFlow.Interfaces.Repository;

public interface IContainerRepository
{
    Task<List<ContainerToDevicesDTO>> GetContainersAsync(int userId);
    Task<List<ContainerToDevicesDTO>> GetContainersWithNoSessionAsync(int userId);
    Task<Container?> GetContainerByIdAsync(int id);
    Task<Container?> GetOpenContainerByUserIdAsync(int userId);
    Task<int> RegisterContainerAsync(Container container);
    Task<bool> UpdateContainerAsync(ContainerEditRequest request);

    Task<bool> AddDevicesInContainersAsync(List<AddDeviceRequest> devices, int containerId);
}
