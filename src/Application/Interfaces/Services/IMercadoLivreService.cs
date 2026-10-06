using API_RouteXFlow.Responses;

namespace API_RouteXFlow.Interfaces.Services;

public interface IMercadoLivreService
{
    Task<CustomResponse<string>> GetAuthorizationUrlAsync(int userId);
    Task<CustomResponse<string>> HandleOAuthCallbackAsync(string code, string state);
    Task<CustomResponse<MercadoLivreUserResponse>> GetUserInfoAsync(int userId);
}
