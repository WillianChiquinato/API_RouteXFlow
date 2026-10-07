using System.Security.Claims;
using API_RouteXFlow.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_RouteXFlow.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuditController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    [Route("getLogs")]
    public async Task<IActionResult> GetLogs([FromQuery] AuditLogFilterRequest filter)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var result = await _auditLogService.SearchAsync(userId, filter);
        return result.Success ? Ok(result) : StatusCode(StatusCodes.Status403Forbidden, result);
    }

    [HttpGet]
    [Route("getLog/{id:int}")]
    public async Task<IActionResult> GetLog(int id)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var result = await _auditLogService.GetByIdAsync(userId, id);
        if (result.Success) return Ok(result);

        return result.Errors.Any(e => e.StartsWith("Acesso restrito"))
            ? StatusCode(StatusCodes.Status403Forbidden, result)
            : NotFound(result);
    }

    private bool TryGetUserId(out int userId) =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
}
