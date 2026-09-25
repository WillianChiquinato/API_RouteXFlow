using API_RouteXFlow.Domain.Data.Entities;

namespace API_RouteXFlow.Interfaces.Repository;

public interface IWorkSessionRepository
{
    Task<List<ContainerToDevicesDTO>> GetActiveContainersWithDevicesAsync(int userId);
    Task<WorkSession?> GetOpenWorkSessionAsync(int userId);
    Task<WorkSession?> GetWorkSessionByIdAsync(int id);
    Task<WorkSession> CreateWorkSessionAsync(WorkSession session);
    Task<GpsPositionHistory> AddGpsPositionAsync(GpsPositionHistory position);
    Task<bool> FinishWorkSessionAsync(int id, DateTime endTime);
    Task<List<WorkSessionSummaryResponse>> GetSessionsAsync(int userId, WorkSessionFilterRequest filter);
    Task<WorkSessionDetailResponse?> GetSessionDetailAsync(int userId, int id);
}
