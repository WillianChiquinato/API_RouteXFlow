using API_RouteXFlow.Domain.Data.Entities;
using API_RouteXFlow.Interfaces.Repository;
using API_RouteXFlow.Interfaces.Services;
using API_RouteXFlow.Responses;
using Microsoft.Extensions.Logging;

namespace API_RouteXFlow.Services;

public class DeviceService : IDeviceService
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IContainerRepository _containerRepository;
    private readonly ILogger<DeviceService> _logger;

    public DeviceService(IDeviceRepository deviceRepository, IContainerRepository containerRepository, ILogger<DeviceService> logger)
    {
        _deviceRepository = deviceRepository;
        _containerRepository = containerRepository;
        _logger = logger;
    }

    public async Task<CustomResponse<List<Device>>> GetDevices()
    {
        try
        {
            var devices = await _deviceRepository.GetDevicesAsync();
            
            if (!devices.Any())
            {
                _logger.LogError("Nenhum device encontrado no GetDevices");
                return new CustomResponse<List<Device>>(false, new List<string> { "Nenhum device encontrado." }, new List<Device>());
            }
            
            return new CustomResponse<List<Device>>(true, new List<string>(), devices);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<CustomResponse<bool>> RegisterAsync(DeviceRegisterRequest request, int userId)
    {
        try
        {
            var container = await _containerRepository.GetContainerByIdAsync(request.ContainerIdVinculated);

            if (container is null)
            {
                _logger.LogError("Container {ContainerId} não encontrado para vincular device", request.ContainerIdVinculated);
                return new CustomResponse<bool>(false, new List<string> { "Container não encontrado." }, false);
            }

            if (container.UserId != userId)
            {
                _logger.LogWarning("Usuário {UserId} tentou registrar device em container de outro usuário", userId);
                return new CustomResponse<bool>(false, new List<string> { "Container não pertence ao usuário autenticado." }, false);
            }

            var registered = await _deviceRepository.RegisterDeviceAsync(request);

            return new CustomResponse<bool>(registered, new List<string>(), registered);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Erro ao registrar device");
            throw;
        }
    }

    public async Task<CustomResponse<bool>> EditAsync(DeviceEditRequest request, int userId)
    {
        try
        {
            var ownerUserId = await _deviceRepository.GetOwnerUserIdByDeviceIdAsync(request.Id);

            if (ownerUserId is null)
            {
                _logger.LogError("Device {DeviceId} não encontrado para atualização", request.Id);
                return new CustomResponse<bool>(false, new List<string> { "Device não encontrado." }, false);
            }

            if (ownerUserId != userId)
            {
                _logger.LogWarning("Usuário {UserId} tentou atualizar device de outro usuário", userId);
                return new CustomResponse<bool>(false, new List<string> { "Device não pertence ao usuário autenticado." }, false);
            }

            var updated = await _deviceRepository.UpdateDeviceAsync(request);

            return new CustomResponse<bool>(updated, new List<string>(), updated);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Erro ao atualizar device");
            throw;
        }
    }

    public async Task<CustomResponse<bool>> DeleteAsync(int id, int userId)
    {
        try
        {
            var ownerUserId = await _deviceRepository.GetOwnerUserIdByDeviceIdAsync(id);

            if (ownerUserId is null)
            {
                _logger.LogError("Device {DeviceId} não encontrado para exclusão", id);
                return new CustomResponse<bool>(false, new List<string> { "Device não encontrado." }, false);
            }

            if (ownerUserId != userId)
            {
                _logger.LogWarning("Usuário {UserId} tentou excluir device de outro usuário", userId);
                return new CustomResponse<bool>(false, new List<string> { "Device não pertence ao usuário autenticado." }, false);
            }

            var deleted = await _deviceRepository.DeleteDeviceAsync(id);

            return new CustomResponse<bool>(deleted, new List<string>(), deleted);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Erro ao excluir device");
            throw;
        }
    }
}