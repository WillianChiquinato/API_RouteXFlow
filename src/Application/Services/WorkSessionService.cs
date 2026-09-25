using API_RouteXFlow.Domain.Data.Entities;
using API_RouteXFlow.Interfaces.Repository;
using API_RouteXFlow.Interfaces.Services;
using API_RouteXFlow.Responses;
using Microsoft.Extensions.Logging;

namespace API_RouteXFlow.Services;

public class WorkSessionService : IWorkSessionService
{
    private readonly IWorkSessionRepository _workSessionRepository;
    private readonly ILogger<WorkSessionService> _logger;

    public WorkSessionService(IWorkSessionRepository workSessionRepository, ILogger<WorkSessionService> logger)
    {
        _workSessionRepository = workSessionRepository;
        _logger = logger;
    }

    public async Task<CustomResponse<WorkSessionSummaryResponse>> StartAsync(WorkSessionRequest request, int userId)
    {
        try
        {
            var openSession = await _workSessionRepository.GetOpenWorkSessionAsync(userId);

            if (openSession is not null)
            {
                _logger.LogWarning("Usuário {UserId} tentou iniciar uma corrida com outra já em andamento", userId);
                return new CustomResponse<WorkSessionSummaryResponse>(false, new List<string> { "Já existe uma corrida em andamento." }, null);
            }

            var containers = await _workSessionRepository.GetActiveContainersWithDevicesAsync(userId);

            if (!containers.Any())
            {
                return new CustomResponse<WorkSessionSummaryResponse>(false, new List<string> { "Nenhum container ativo encontrado." }, null);
            }

            if (containers.Count > 1)
            {
                _logger.LogWarning("Usuário {UserId} possui mais de um container ativo", userId);
                return new CustomResponse<WorkSessionSummaryResponse>(false, new List<string> { "Mais de um container ativo encontrado. Ative apenas um container antes de iniciar a corrida." }, null);
            }

            var container = containers[0];

            var hasOwnerConnected = container.Devices.Any(device => device.Type == DeviceType.Manager && device.Connected);
            var hasNavigationConnected = container.Devices.Any(device => device.Type == DeviceType.Navigation && device.Connected);

            if (!hasOwnerConnected)
            {
                return new CustomResponse<WorkSessionSummaryResponse>(false, new List<string> { "Nenhum aparelho owner conectado no container." }, null);
            }

            if (!hasNavigationConnected)
            {
                return new CustomResponse<WorkSessionSummaryResponse>(false, new List<string> { "Nenhum aparelho navigation conectado no container." }, null);
            }

            var session = new WorkSession
            {
                UserId = userId,
                ContainerId = container.Id,
                StartTime = DateTime.UtcNow
            };

            var createdSession = await _workSessionRepository.CreateWorkSessionAsync(session);

            await _workSessionRepository.AddGpsPositionAsync(new GpsPositionHistory
            {
                WorkSessionId = createdSession.Id,
                TypePosition = TypePosition.StartPosition,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Timestamp = DateTime.UtcNow
            });

            var response = new WorkSessionSummaryResponse
            {
                Id = createdSession.Id,
                StartTime = createdSession.StartTime,
                EndTime = createdSession.EndTime,
                ContainerId = container.Id,
                ContainerName = container.Name,
                DeliveryOffersCount = 0
            };

            return new CustomResponse<WorkSessionSummaryResponse>(true, new List<string>(), response);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Erro ao iniciar corrida");
            throw;
        }
    }

    public async Task<CustomResponse<WorkSessionSummaryResponse>> FinishAsync(int workSessionId, WorkSessionRequest request, int userId)
    {
        try
        {
            var session = await _workSessionRepository.GetWorkSessionByIdAsync(workSessionId);

            if (session is null)
            {
                return new CustomResponse<WorkSessionSummaryResponse>(false, new List<string> { "Corrida não encontrada." }, null);
            }

            if (session.UserId != userId)
            {
                _logger.LogWarning("Usuário {UserId} tentou finalizar corrida de outro usuário", userId);
                return new CustomResponse<WorkSessionSummaryResponse>(false, new List<string> { "Corrida não pertence ao usuário autenticado." }, null);
            }

            if (session.EndTime is not null)
            {
                return new CustomResponse<WorkSessionSummaryResponse>(false, new List<string> { "Esta corrida já foi finalizada." }, null);
            }

            var endTime = DateTime.UtcNow;
            var finished = await _workSessionRepository.FinishWorkSessionAsync(workSessionId, endTime);

            if (!finished)
            {
                return new CustomResponse<WorkSessionSummaryResponse>(false, new List<string> { "Erro ao finalizar a corrida." }, null);
            }

            await _workSessionRepository.AddGpsPositionAsync(new GpsPositionHistory
            {
                WorkSessionId = workSessionId,
                TypePosition = TypePosition.FinishedPosition,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Timestamp = endTime
            });

            var detail = await _workSessionRepository.GetSessionDetailAsync(userId, workSessionId);

            var response = new WorkSessionSummaryResponse
            {
                Id = workSessionId,
                StartTime = session.StartTime,
                EndTime = endTime,
                ContainerId = session.ContainerId,
                ContainerName = detail?.ContainerName ?? string.Empty,
                DeliveryOffersCount = detail?.DeliveryOffersCount ?? 0
            };

            return new CustomResponse<WorkSessionSummaryResponse>(true, new List<string>(), response);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Erro ao finalizar corrida");
            throw;
        }
    }

    public async Task<CustomResponse<List<WorkSessionSummaryResponse>>> GetSessions(WorkSessionFilterRequest filter, int userId)
    {
        try
        {
            var sessions = await _workSessionRepository.GetSessionsAsync(userId, filter);

            return new CustomResponse<List<WorkSessionSummaryResponse>>(true, new List<string>(), sessions);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Erro ao buscar histórico de corridas");
            throw;
        }
    }

    public async Task<CustomResponse<WorkSessionDetailResponse>> GetSessionDetail(int id, int userId)
    {
        try
        {
            var detail = await _workSessionRepository.GetSessionDetailAsync(userId, id);

            if (detail is null)
            {
                return new CustomResponse<WorkSessionDetailResponse>(false, new List<string> { "Corrida não encontrada." }, null);
            }

            return new CustomResponse<WorkSessionDetailResponse>(true, new List<string>(), detail);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Erro ao buscar detalhe da corrida");
            throw;
        }
    }
}
