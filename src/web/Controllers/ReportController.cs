using Application.Interfaces;
using Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using web.Extensions;

namespace web.Controllers;

[ApiController]
[Authorize]
[Route("api/reports")]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    // Ventas de hoy (dueño: todas sus sucursales; empleado: las que registró)
    [HttpGet("today")]
    public ActionResult<TodayResponse> GetToday()
    {
        return Ok(_reportService.GetToday(User.GetCuit()));
    }

    // Sin companyId ni branchId se toman todas las sucursales del dueño
    [Authorize(Roles = "Owner")]
    [HttpGet("ranking")]
    public ActionResult<RankingResponse> GetRanking(Guid? companyId, Guid? branchId, int year, int month, string order = "most")
    {
        return Ok(_reportService.GetRanking(User.GetCuit(), companyId, branchId, year, month, order != "least"));
    }

    [Authorize(Roles = "Owner")]
    [HttpGet("monthly-close")]
    public async Task<ActionResult<MonthlyCloseResponse>> GetMonthlyClose(Guid? companyId, Guid? branchId, int year, int month)
    {
        return Ok(await _reportService.GetMonthlyCloseAsync(User.GetCuit(), companyId, branchId, year, month));
    }
}