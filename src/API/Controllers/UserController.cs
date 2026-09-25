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
}