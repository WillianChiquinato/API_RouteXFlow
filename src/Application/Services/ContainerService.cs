using API_RouteXFlow.Domain.Data.Entities;
using API_RouteXFlow.Interfaces.Repository;
using API_RouteXFlow.Interfaces.Services;
using API_RouteXFlow.Responses;
using Microsoft.Extensions.Logging;

namespace API_RouteXFlow.Services;

public class ContainerService : IContainerService
{
    private readonly IContainerRepository _containerRepository;
    private readonly ILogger<ContainerService> _logger;

    public ContainerService(IContainerRepository containerRepository, ILogger<ContainerService> logger)
    {
        _containerRepository = containerRepository;
        _logger = logger;
    }

    public async Task<CustomResponse<List<ContainerToDevicesDTO>>> GetContainers()
    {
        try
        {
            var containers = await _containerRepository.GetContainersAsync();

            if (!containers.Any())
            {
                _logger.LogError("Nenhum container encontrado no GetContainers");
                return new CustomResponse<List<ContainerToDevicesDTO>>(false, new List<string> { "Nenhum container encontrado." }, new List<ContainerToDevicesDTO>());
            }

            return new CustomResponse<List<ContainerToDevicesDTO>>(true, new List<string>(), containers);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<CustomResponse<bool>> RegisterAsync(ContainerRegisterRequest containerRegisterRequest, int userId)
    {
        try
        {
            var container = new Container
            {
                Name = containerRegisterRequest.Name,
                IsActive = true,
                UserId = userId
            };

            var containerId = await _containerRepository.RegisterContainerAsync(container);
            var devicesRegisters = await _containerRepository.AddDevicesInContainersAsync(containerRegisterRequest.Devices, containerId);

            if (!devicesRegisters)
            {
                return new CustomResponse<bool>(false, new List<string> { "Erro ao criar relação entre devices" }, false);
            }
            
            return new CustomResponse<bool>(true, new List<string>(), containerId > 0);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Erro ao registrar container");
            throw;
        }
    }

    public async Task<CustomResponse<bool>> EditAsync(ContainerEditRequest containerEditRequest, int userId)
    {
        try
        {
            var container = await _containerRepository.GetContainerByIdAsync(containerEditRequest.Id);

            if (container is null)
            {
                _logger.LogError("Container {ContainerId} não encontrado para atualização", containerEditRequest.Id);
                return new CustomResponse<bool>(false, new List<string> { "Container não encontrado." }, false);
            }

            if (container.UserId != userId)
            {
                _logger.LogWarning("Usuário {UserId} tentou atualizar container de outro usuário", userId);
                return new CustomResponse<bool>(false, new List<string> { "Container não pertence ao usuário autenticado." }, false);
            }

            var updated = await _containerRepository.UpdateContainerAsync(containerEditRequest);

            return new CustomResponse<bool>(updated, new List<string>(), updated);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Erro ao atualizar container");
            throw;
        }
    }
}
