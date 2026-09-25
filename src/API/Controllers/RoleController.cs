using API_RouteXFlow.Domain.Data.Entities;
using API_RouteXFlow.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_RouteXFlow.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RoleController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    [Route("getRoles")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _roleService.GetRoles();
        
        return roles.Result != null ? Ok(roles) : BadRequest(roles);
    }
}