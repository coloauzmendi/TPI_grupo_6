using Application.Interfaces;
using Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using web.Extensions;

namespace web.Controllers;

[ApiController]
[Authorize(Roles = "Owner,Employee")]
[Route("api/branch/{branchId}/sales")]
public class SaleController : ControllerBase
{
    private readonly ISaleService _saleService;

    public SaleController(ISaleService saleService)
    {
        _saleService = saleService;
    }

    // Registra la venta y descuenta el stock; devuelve el comprobante y los avisos de stock bajo
    [HttpPost]
    public ActionResult<SaleResultResponse> Create(Guid branchId, SaleRequest request)
    {
        return StatusCode(StatusCodes.Status201Created, _saleService.Create(User.GetCuit(), branchId, request));
    }

    // Anula la venta y devuelve el stock; el empleado solo puede anular las suyas
    [HttpDelete("{saleId}")]
    public IActionResult Cancel(Guid branchId, Guid saleId)
    {
        _saleService.Cancel(User.GetCuit(), branchId, saleId);
        return NoContent();
    }
}