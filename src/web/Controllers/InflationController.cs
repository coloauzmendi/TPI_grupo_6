using Application.Interfaces;
using Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace web.Controllers;

[ApiController]
[Authorize]
[Route("api/inflation")]
public class InflationController : ControllerBase
{
    private readonly IInflationRateService _inflationRateService;

    public InflationController(IInflationRateService inflationRateService)
    {
        _inflationRateService = inflationRateService;
    }

    // Inflación mensual del último mes publicado por el INDEC
    [HttpGet("latest")]
    public async Task<ActionResult<InflationRateResponse>> GetLatest(CancellationToken cancellationToken)
    {
        return Ok(await _inflationRateService.GetLatestAsync(cancellationToken));
    }
}
