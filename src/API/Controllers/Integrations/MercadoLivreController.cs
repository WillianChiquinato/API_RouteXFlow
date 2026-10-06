using System.Security.Claims;
using API_RouteXFlow.Interfaces.Services;
using API_RouteXFlow.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_RouteXFlow.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MercadoLivreController : ControllerBase
{
    private readonly IMercadoLivreService _mercadoLivreService;

    public MercadoLivreController(IMercadoLivreService mercadoLivreService)
    {
        _mercadoLivreService = mercadoLivreService;
    }

    [HttpGet]
    [Route("oauth/authorize")]
    public async Task<IActionResult> Authorize()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userId))
            return Unauthorized(CustomResponse<string>.Fail("Usuário não autenticado."));

        var result = await _mercadoLivreService.GetAuthorizationUrlAsync(int.Parse(userId));

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet]
    [Route("oauth/users/me")]
    public async Task<IActionResult> GetUserInfo()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userId))
            return Unauthorized(CustomResponse<string>.Fail("Usuário não autenticado."));

        var result = await _mercadoLivreService.GetUserInfoAsync(int.Parse(userId));

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet]
    [Route("oauth/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> OAuthCallback([FromQuery] string code, [FromQuery] string state)
    {
        var result = await _mercadoLivreService.HandleOAuthCallbackAsync(code, state);

        return result.Success ? Ok(result) : BadRequest(result);
    }
}
