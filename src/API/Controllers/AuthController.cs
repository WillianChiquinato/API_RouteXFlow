

using System.Security.Claims;
using API_RouteXFlow.Domain.Data.Entities;
using API_RouteXFlow.Interfaces.Services;
using API_RouteXFlow.Responses;
using API_RouteXFlow.Services;
using Application.DTO.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_RouteXFlow.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly TokenService _tokenService;

    public AuthController(IAuthService authService, TokenService tokenService)
    {
        _authService = authService;
        _tokenService = tokenService;
    }

    [HttpPost]
    [Route("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var userIdentify = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub")
                     ?? string.Empty;

        if (!string.IsNullOrEmpty(userIdentify))
        {
            var response = new CustomResponse<string>(false, new List<string>
            {
                "Usuário ja autenticado, não é possível realizar login novamente simultaneamente."
            }, null);
            return Unauthorized(response);
        }

        var auth = await _authService.LoginAsync(request);

        if (auth.Success && auth.Result is not null)
            _tokenService.AppendAuthCookie(Response, auth.Result, Request.IsHttps);

        return auth.Success
            ? Ok(auth)
            : NotFound(auth);
    }
    
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userId))
        {
            var response = new CustomResponse<string>(false, new List<string>
            {
                "Usuário não autenticado."
            }, null);
            return Unauthorized(response);
        }
        
        var user = await _authService.SearchUserByIdAsync(int.Parse(userId));

        var me = new MeResponse
        {
            Id = userId,
            Name = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue("name"),
            Email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email"),
            Role = User.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList(),
            User = user.Result ?? new User()
        };

        return Ok(new CustomResponse<MeResponse>(true, new List<string>(), me));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh()
    {
        var token = GetToken();
        var auth = await _authService.RefreshAsync(token);

        if (!auth.Success)
            return Unauthorized(auth);

        if (auth.Result is not null)
            _tokenService.AppendAuthCookie(Response, auth.Result, Request.IsHttps);

        return Ok(auth);
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public IActionResult Logout()
    {
        _tokenService.ClearAuthCookie(Response, Request.IsHttps);
        return NoContent();
    }

    private string GetToken()
    {
        if (Request.Cookies.TryGetValue(TokenService.AuthCookieName, out var cookieToken))
            return cookieToken;

        var authorization = Request.Headers.Authorization.ToString();
        return authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authorization["Bearer ".Length..].Trim()
            : string.Empty;
    }
}