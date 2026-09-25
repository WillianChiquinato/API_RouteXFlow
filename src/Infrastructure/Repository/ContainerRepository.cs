using API_RouteXFlow.Domain.Data.Entities;
using API_RouteXFlow.Interfaces.Repository;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace API_RouteXFlow.Repository;

public class ContainerRepository : IContainerRepository
{
    private readonly AppDbContext _dbContext;

    public ContainerRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ContainerToDevicesDTO>> GetContainersAsync()
    {
        return await _dbContext.Containers
            .AsNoTracking()
            .Select(container => new ContainerToDevicesDTO
            {
                Id = container.Id,
                Name = container.Name,
                IsActive = container.IsActive,
                Devices = container.ContainerDevices
                    .Where(containerDevice => containerDevice.IsActive)
                    .Select(containerDevice => containerDevice.Device!)
                    .ToList()
            })
            .ToListAsync();
    }

    public async Task<Container?> GetContainerByIdAsync(int id)
    {
        return await _dbContext.Containers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<int> RegisterContainerAsync(Container container)
    {
        _dbContext.Add(container);
        await _dbContext.SaveChangesAsync();
        return container.Id;
    }

    public async Task<bool> UpdateContainerAsync(ContainerEditRequest request)
    {
        var container = await _dbContext.Containers.FirstOrDefaultAsync(c => c.Id == request.Id);

        if (container is null)
        {
            return false;
        }

        container.Name = request.Name;
        container.IsActive = request.isActive;

        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> AddDevicesInContainersAsync(List<AddDeviceRequest> devices, int containerId)
    {
        var vinculatedEntities = devices.Select(d => new ContainerDevices()
        {
            ContainerId = containerId,
            PairedAt = DateTime.UtcNow,
            LastConnectedAt = DateTime.UtcNow,
            IsActive = true,
            Device = new Device 
            {
                Name = d.Name,
                DeviceIdentifier = d.Role,
                Type = d.Type == 1 ? DeviceType.Manager : DeviceType.Navigation,
                Connected = false
            }
        }).ToList();

        await _dbContext.ContainerDevices.AddRangeAsync(vinculatedEntities);
        await _dbContext.SaveChangesAsync();

        return true;
    }
}
