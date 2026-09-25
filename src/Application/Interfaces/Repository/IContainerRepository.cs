using API_RouteXFlow.Domain.Data.Entities;

namespace API_RouteXFlow.Interfaces.Repository;

public interface IContainerRepository
{
    Task<List<ContainerToDevicesDTO>> GetContainersAsync();
    Task<Container?> GetContainerByIdAsync(int id);
    Task<int> RegisterContainerAsync(Container container);
    Task<bool> UpdateContainerAsync(ContainerEditRequest request);

    Task<bool> AddDevicesInContainersAsync(List<AddDeviceRequest> devices, int containerId);
}
