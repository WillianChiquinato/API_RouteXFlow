using System.Security.Claims;
using API_RouteXFlow.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_RouteXFlow.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubAccountController : ControllerBase
{
    private readonly ISubAccountService _subAccountService;

    public SubAccountController(ISubAccountService subAccountService)
    {
        _subAccountService = subAccountService;
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

    private bool TryGetUserId(out int userId)
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return int.TryParse(claim, out userId);
    }
}
