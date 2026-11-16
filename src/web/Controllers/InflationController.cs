using Application.Interfaces;
using Application.Models;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace web.Controllers;

[ApiController]
[Authorize]
[Route("api/inflation")]
public class InflationController : ControllerBase
{
    private readonly IInflationService _inflationService;

    public InflationController(IInflationService inflationService)
    {
        _inflationService = inflationService;
    }

    // Inflación mensual del último mes publicado por el INDEC
    [HttpGet("latest")]
    public async Task<ActionResult<InflationRateResponse>> GetLatest(CancellationToken cancellationToken)
    {
        InflationSeries? series = await _inflationService.GetSeriesAsync(cancellationToken);
        InflationRateResponse? latest = series?.Latest();

        if (latest is null)
        {
            throw new ExternalServiceException("No se pudo obtener la inflación desde datos.gob.ar.");
        }

        return Ok(latest);
    }
}
