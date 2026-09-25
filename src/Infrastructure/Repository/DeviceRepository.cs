using API_RouteXFlow.Domain.Data.Entities;
using API_RouteXFlow.Interfaces.Repository;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace API_RouteXFlow.Repository;

public class DeviceRepository : IDeviceRepository
{
    private readonly AppDbContext _dbContext;

    public DeviceRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Device>> GetDevicesAsync()
    {
        return await _dbContext.Devices.AsNoTracking().ToListAsync();
    }

    public async Task<bool> RegisterDeviceAsync(DeviceRegisterRequest request)
    {
        var containerDevice = new ContainerDevices
        {
            ContainerId = request.ContainerIdVinculated,
            PairedAt = DateTime.UtcNow,
            LastConnectedAt = DateTime.UtcNow,
            IsActive = true,
            Device = new Device
            {
                Name = request.Name,
                DeviceIdentifier = request.Role,
                Type = request.Type == 1 ? DeviceType.Manager : DeviceType.Navigation,
                Connected = false
            }
        };

        _dbContext.Add(containerDevice);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<int?> GetOwnerUserIdByDeviceIdAsync(int deviceId)
    {
        return await _dbContext.ContainerDevices
            .AsNoTracking()
            .Where(containerDevice => containerDevice.DeviceId == deviceId)
            .Select(containerDevice => (int?)containerDevice.Container!.UserId)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> UpdateDeviceAsync(DeviceEditRequest request)
    {
        var device = await _dbContext.Devices.FirstOrDefaultAsync(d => d.Id == request.Id);

        if (device is null)
        {
            return false;
        }

        device.Name = request.Name;
        device.DeviceIdentifier = request.Role;
        device.Type = request.Type == 1 ? DeviceType.Manager : DeviceType.Navigation;

        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteDeviceAsync(int id)
    {
        var device = await _dbContext.Devices.FirstOrDefaultAsync(d => d.Id == id);

        if (device is null)
        {
            return false;
        }

        _dbContext.Devices.Remove(device);
        await _dbContext.SaveChangesAsync();

        return true;
    }
}