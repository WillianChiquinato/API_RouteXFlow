

using System.Security.Claims;
using API_RouteXFlow.Interfaces.Services;
using API_RouteXFlow.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_RouteXFlow.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost]
    [Route("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var userIdentify = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub")
                     ?? string.Empty;

        if (string.IsNullOrEmpty(userIdentify))
        {
            var response = new CustomResponse<string>(false, new List<string>
            {
                "Usuário ja autenticado, não é possível realizar login novamente simultaneamente."
            }, null);
            return Unauthorized(response);
        }

        var auth = await _authService.LoginAsync(request);

        return auth.Success
            ? Ok(auth)
            : Unauthorized(auth);
    }
}