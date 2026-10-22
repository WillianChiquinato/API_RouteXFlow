using System.Security.Claims;
using API_RouteXFlow.Interfaces.Services;
using API_RouteXFlow.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_RouteXFlow.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubAccountController : ControllerBase
{
    private readonly ISubAccountService _subAccountService;
    private readonly IAuthService _authService;
    private readonly TokenService _tokenService;

    public SubAccountController(ISubAccountService subAccountService, IAuthService authService, TokenService tokenService)
    {
        _subAccountService = subAccountService;
        _authService = authService;
        _tokenService = tokenService;
    }

    [HttpPost]
    [Route("loginInApps")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginInApps(LoginInAppsRequest request)
    {
        var authInApps = await _authService.LoginInAppsAsync(request);

        if (authInApps.Success && authInApps.Result is not null)
            _tokenService.AppendSubAccountAuthCookie(Response, authInApps.Result, Request.IsHttps);

        return authInApps.Success
            ? Ok(authInApps)
            : NotFound(authInApps);
    }

    // Independe da sessão do dono: valida só o token da sub conta (cookie próprio ou Bearer).
    [HttpGet]
    [Route("me")]
    [AllowAnonymous]
    public async Task<IActionResult> Me()
    {
        var subAccount = _tokenService.GetSubAccountFromToken(GetToken());
        if (subAccount == null) return Unauthorized();

        var response = await _authService.GetSubAccountMeAsync(subAccount.Id, subAccount.OwnerUserId);
        return response.Success ? Ok(response) : Unauthorized(response);
    }

    [HttpPost]
    [Route("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh()
    {
        var auth = await _authService.RefreshSubAccountAsync(GetToken());

        if (!auth.Success)
            return Unauthorized(auth);

        if (auth.Result is not null)
            _tokenService.AppendSubAccountAuthCookie(Response, auth.Result, Request.IsHttps);

        return Ok(auth);
    }

    [HttpPost]
    [Route("logout")]
    [AllowAnonymous]
    public IActionResult Logout()
    {
        _tokenService.ClearSubAccountAuthCookie(Response, Request.IsHttps);
        return NoContent();
    }

    [HttpGet]
    [Route("list")]
    public async Task<IActionResult> List()
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        return Ok(await _subAccountService.ListAsync(userId));
    }

    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> Create(CreateSubAccountRequest request)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var response = await _subAccountService.CreateAsync(userId, request);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpDelete]
    [Route("delete/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var response = await _subAccountService.DeleteAsync(userId, id);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    // Só o cookie da sub conta (nunca o do dono) ou o header Bearer.
    private string GetToken()
    {
        if (Request.Cookies.TryGetValue(TokenService.SubAccountCookieName, out var cookieToken))
            return cookieToken;

        var authorization = Request.Headers.Authorization.ToString();
        return authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authorization["Bearer ".Length..].Trim()
            : string.Empty;
    }

    private bool TryGetUserId(out int userId)
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return int.TryParse(claim, out userId);
    }
}
