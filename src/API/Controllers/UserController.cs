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
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    [Route("register")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterUser(UserRegisterRequest request)
    {
        var user = await _userService.RegisterAsync(request);
        
        return Ok(user);
    }

    [HttpPut]
    [Route("profile")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var response = await _userService.UpdateProfileAsync(userId, request);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpPut]
    [Route("preferences")]
    public async Task<IActionResult> UpdatePreferences(UpdatePreferencesRequest request)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var response = await _userService.UpdatePreferencesAsync(userId, request);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpPut]
    [Route("password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var response = await _userService.ChangePasswordAsync(userId, request);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpGet]
    [Route("profile")]
    public async Task<IActionResult> GetProfile()
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        
        var response = await _userService.GetProfileAsync(userId);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    private bool TryGetUserId(out int userId)
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return int.TryParse(claim, out userId);
    }
}
