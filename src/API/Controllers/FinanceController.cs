using System.Security.Claims;
using API_RouteXFlow.Interfaces.Services;
using API_RouteXFlow.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_RouteXFlow.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FinanceController : ControllerBase
{
    private readonly IFinanceService _financeService;

    public FinanceController(IFinanceService financeService)
    {
        _financeService = financeService;
    }

    [HttpGet]
    [Route("getEntries")]
    public async Task<IActionResult> GetEntries([FromQuery] FinanceFilterRequest filter)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userId))
        {
            var response = new CustomResponse<string>(false, new List<string> { "Usuário não autenticado." }, null);
            return Unauthorized(response);
        }

        var entries = await _financeService.GetEntries(filter, int.Parse(userId));

        return entries.Result != null ? Ok(entries) : BadRequest(entries);
    }

    [HttpGet]
    [Route("getSummary")]
    public async Task<IActionResult> GetSummary([FromQuery] FinanceFilterRequest filter)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userId))
        {
            var response = new CustomResponse<string>(false, new List<string> { "Usuário não autenticado." }, null);
            return Unauthorized(response);
        }

        var summary = await _financeService.GetSummary(filter, int.Parse(userId));

        return summary.Success ? Ok(summary) : BadRequest(summary);
    }

    [HttpPost]
    [Route("registerEntry")]
    public async Task<IActionResult> RegisterEntry(FinanceEntryRegisterRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userId))
        {
            var response = new CustomResponse<string>(false, new List<string> { "Usuário não autenticado." }, null);
            return Unauthorized(response);
        }

        var entry = await _financeService.RegisterEntry(request, int.Parse(userId));

        return entry.Success ? Ok(entry) : BadRequest(entry);
    }

    [HttpPut]
    [Route("updateEntry")]
    public async Task<IActionResult> UpdateEntry(FinanceEntryUpdateRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userId))
        {
            var response = new CustomResponse<string>(false, new List<string> { "Usuário não autenticado." }, null);
            return Unauthorized(response);
        }

        var entry = await _financeService.UpdateEntry(request, int.Parse(userId));

        return entry.Success ? Ok(entry) : BadRequest(entry);
    }

    [HttpDelete]
    [Route("deleteEntry/{id:int}")]
    public async Task<IActionResult> DeleteEntry(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userId))
        {
            var response = new CustomResponse<string>(false, new List<string> { "Usuário não autenticado." }, null);
            return Unauthorized(response);
        }

        var entry = await _financeService.DeleteEntry(id, int.Parse(userId));

        return entry.Success ? Ok(entry) : BadRequest(entry);
    }

    [HttpGet]
    [Route("getMonthClosure")]
    public async Task<IActionResult> GetMonthClosure([FromQuery] FinanceMonthPeriodRequest period)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userId))
        {
            var response = new CustomResponse<string>(false, new List<string> { "Usuário não autenticado." }, null);
            return Unauthorized(response);
        }

        var closure = await _financeService.GetMonthClosure(period, int.Parse(userId));

        return Ok(closure);
    }

    [HttpPost]
    [Route("closeMonth")]
    public async Task<IActionResult> CloseMonth(FinanceMonthPeriodRequest period)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userId))
        {
            var response = new CustomResponse<string>(false, new List<string> { "Usuário não autenticado." }, null);
            return Unauthorized(response);
        }

        var closure = await _financeService.CloseMonth(period, int.Parse(userId));

        return closure.Success ? Ok(closure) : BadRequest(closure);
    }

    [HttpPost]
    [Route("generateAiReport")]
    public async Task<IActionResult> GenerateAiReport(FinanceMonthPeriodRequest period)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userId))
        {
            var response = new CustomResponse<string>(false, new List<string> { "Usuário não autenticado." }, null);
            return Unauthorized(response);
        }

        var report = await _financeService.GenerateAiReport(period, int.Parse(userId));

        return report.Success ? Ok(report) : BadRequest(report);
    }
}
