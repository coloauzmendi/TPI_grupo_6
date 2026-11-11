using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using web.Extensions;

namespace web.Controllers;

[ApiController]
[Authorize(Roles = "Employee")]
[Route("api/branch/{branchId}/sales")]
public class SaleController : ControllerBase
{
    private readonly SaleService _saleService;

    public SaleController(SaleService saleService)
    {
        _saleService = saleService;
    }

    // Registra la venta y descuenta el stock; devuelve el comprobante y los avisos de stock bajo
    [HttpPost]
    public ActionResult<SaleResultResponse> Create(Guid branchId, SaleRequest request)
    {
        return Ok(_saleService.Create(User.GetCuit(), branchId, request));
    }
}