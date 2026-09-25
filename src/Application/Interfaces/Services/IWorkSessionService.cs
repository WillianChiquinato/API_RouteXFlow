using API_RouteXFlow.Responses;

namespace API_RouteXFlow.Interfaces.Services;

public interface IWorkSessionService
{
    Task<CustomResponse<WorkSessionSummaryResponse>> StartAsync(WorkSessionRequest request, int userId);
    Task<CustomResponse<WorkSessionSummaryResponse>> FinishAsync(int workSessionId, WorkSessionRequest request, int userId);
    Task<CustomResponse<List<WorkSessionSummaryResponse>>> GetSessions(WorkSessionFilterRequest filter, int userId);
    Task<CustomResponse<WorkSessionDetailResponse>> GetSessionDetail(int id, int userId);
}
