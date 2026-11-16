using Application.Interfaces;
using Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using web.Extensions;

namespace web.Controllers;

[ApiController]
[Authorize]
[Route("api/branch/{branchId}/stock")]
public class BranchStockController : ControllerBase
{
    private readonly IStockService _stockService;

    public BranchStockController(IStockService stockService)
    {
        _stockService = stockService;
    }

    // Stock activo de la sucursal junto con los datos de cada producto; el dueño puede pedir también el dado de baja
    [HttpGet]
    public ActionResult<List<StockItemResponse>> GetByBranch(Guid branchId, [FromQuery] bool includeInactive = false)
    {
        return Ok(_stockService.GetItems(User.GetCuit(), branchId, includeInactive));
    }

    [HttpGet("{productId}")]
    public ActionResult<BranchStockResponse> GetById(Guid branchId, Guid productId)
    {
        return Ok(_stockService.GetById(User.GetCuit(), branchId, productId));
    }

    [Authorize(Roles = "Owner")]
    [HttpPost]
    public ActionResult<BranchStockResponse> Create(Guid branchId, BranchStockCreateRequest request)
    {
        BranchStockResponse created = _stockService.Create(User.GetCuit(), branchId, request);
        return CreatedAtAction(nameof(GetById), new { branchId, productId = created.ProductId }, created);
    }

    [Authorize(Roles = "Owner")]
    [HttpPut("{productId}")]
    public IActionResult Update(Guid branchId, Guid productId, BranchStockUpdateRequest request)
    {
        _stockService.Update(User.GetCuit(), branchId, productId, request);
        return NoContent();
    }

    [Authorize(Roles = "Owner")]
    [HttpDelete("{productId}")]
    public IActionResult Delete(Guid branchId, Guid productId)
    {
        _stockService.Delete(User.GetCuit(), branchId, productId);
        return NoContent();
    }
}
