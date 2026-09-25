using System.Security.Claims;
using API_RouteXFlow.Interfaces.Services;
using API_RouteXFlow.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_RouteXFlow.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkSessionController : ControllerBase
{
    private readonly IWorkSessionService _workSessionService;

    public WorkSessionController(IWorkSessionService workSessionService)
    {
        _workSessionService = workSessionService;
    }

    [HttpPost]
    [Route("start")]
    public async Task<IActionResult> Start(WorkSessionRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userId))
        {
            var response = new CustomResponse<string>(false, new List<string> { "Usuário não autenticado." }, null);
            return Unauthorized(response);
        }

        var session = await _workSessionService.StartAsync(request, int.Parse(userId));

        return session.Success ? Ok(session) : BadRequest(session);
    }

    [HttpPost]
    [Route("finish/{id:int}")]
    public async Task<IActionResult> Finish(int id, WorkSessionRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userId))
        {
            var response = new CustomResponse<string>(false, new List<string> { "Usuário não autenticado." }, null);
            return Unauthorized(response);
        }

        var session = await _workSessionService.FinishAsync(id, request, int.Parse(userId));

        return session.Success ? Ok(session) : BadRequest(session);
    }

    [HttpGet]
    [Route("getSessions")]
    public async Task<IActionResult> GetSessions([FromQuery] WorkSessionFilterRequest filter)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userId))
        {
            var response = new CustomResponse<string>(false, new List<string> { "Usuário não autenticado." }, null);
            return Unauthorized(response);
        }

        var sessions = await _workSessionService.GetSessions(filter, int.Parse(userId));

        return Ok(sessions);
    }

    [HttpGet]
    [Route("getSession/{id:int}")]
    public async Task<IActionResult> GetSession(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userId))
        {
            var response = new CustomResponse<string>(false, new List<string> { "Usuário não autenticado." }, null);
            return Unauthorized(response);
        }

        var session = await _workSessionService.GetSessionDetail(id, int.Parse(userId));

        return session.Success ? Ok(session) : NotFound(session);
    }
}
