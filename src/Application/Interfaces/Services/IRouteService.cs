using API_RouteXFlow.Responses;

namespace API_RouteXFlow.Interfaces.Services;

public interface IRouteService
{
    Task<CustomResponse<List<RouteSummaryResponse>>> GetRoutes(RouteFilterRequest filter, int userId);
    Task<CustomResponse<RouteDetailResponse>> GetRouteDetail(int id, int userId);
}
