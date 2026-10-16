using System.Security.Claims;
using API_RouteXFlow.Interfaces.Services;
using API_RouteXFlow.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_RouteXFlow.Controllers;

[ApiController]
[Route("api/shoppe")]
[Authorize]
public class ShoppeController : ControllerBase
{
    private readonly IShoppeService _shoppeService;

    public ShoppeController(IShoppeService shoppeService)
    {
        _shoppeService = shoppeService;
    }

    [HttpGet]
    [Route("oauth/sign")]
    public async Task<IActionResult> Authorize()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userId))
            return Unauthorized(CustomResponse<string>.Fail("Usuário não autenticado."));

        var result = await _shoppeService.GetAuthorizationShoppeAsync(int.Parse(userId));

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet]
    [Route("oauth/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> OAuthCallback([FromQuery] string code, [FromQuery] string shop_id, [FromQuery] string partner_id, [FromQuery] string state)
    {
        var result = await _shoppeService.HandleOAuthCallbackAsync(code, shop_id, partner_id, state);
        
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet]
    [Route("getOrdersList")]
    public async Task<IActionResult> GetOrdersList()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userId))
            return Unauthorized(CustomResponse<string>.Fail("Usuário não autenticado."));

        var result = await _shoppeService.GetOrdersListAsync(int.Parse(userId));
        
        return result.Success ? Ok(result) : BadRequest(result);
    }
}