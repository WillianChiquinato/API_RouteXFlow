using API_RouteXFlow.Responses;

namespace API_RouteXFlow.Interfaces.Services;

public interface IShoppeService
{
    Task<CustomResponse<string>> GetAuthorizationShoppeAsync(int userId);
    Task<CustomResponse<string>> HandleOAuthCallbackAsync(string code, string shop_id, string partner_id, string state);
    Task<CustomResponse<ShopeeOrderResponse>> GetOrdersListAsync(int userId);
}