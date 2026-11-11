using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using web.Extensions;

namespace web.Controllers;

// Datos del usuario logueado (se identifica por el CUIT del token)
[ApiController]
[Authorize]
[Route("api/me")]
public class MeController : ControllerBase
{
    private readonly MeService _meService;

    public MeController(MeService meService)
    {
        _meService = meService;
    }

    [HttpGet]
    public ActionResult<MeResponse> GetMe()
    {
        return Ok(_meService.GetMe(User.GetCuit()));
    }

    [HttpGet("branches")]
    public ActionResult<List<BranchResponse>> GetBranches()
    {
        return Ok(_meService.GetBranches(User.GetCuit()));
    }

    [HttpGet("stock-alerts")]
    public ActionResult<List<StockAlertResponse>> GetStockAlerts()
    {
        return Ok(_meService.GetStockAlerts(User.GetCuit()));
    }

    [Authorize(Roles = "Owner")]
    [HttpGet("companies")]
    public ActionResult<List<CompanyDetailResponse>> GetCompanies()
    {
        return Ok(_meService.GetCompanies(User.GetCuit()));
    }

    [Authorize(Roles = "Owner")]
    [HttpGet("employees")]
    public ActionResult<List<EmployeeResponse>> GetEmployees()
    {
        return Ok(_meService.GetEmployees(User.GetCuit()));
    }
}