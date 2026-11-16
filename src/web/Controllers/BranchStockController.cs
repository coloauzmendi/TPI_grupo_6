using Application.Models;
using Application.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using web.Extensions;

namespace web.Controllers;

[ApiController]
[Authorize]
[Route("api/branch/{branchId}/stock")]
public class BranchStockController : ControllerBase
{
    private readonly StockService _stockService;

    public BranchStockController(StockService stockService)
    {
        _stockService = stockService;
    }

    // Stock activo de la sucursal junto con los datos de cada producto
    [HttpGet]
    public ActionResult<List<StockItemResponse>> GetByBranch(Guid branchId)
    {
        return Ok(_stockService.GetItems(User.GetCuit(), branchId));
    }

    [HttpGet("{productId}")]
    public ActionResult<BranchStock> GetById(Guid branchId, Guid productId)
    {
        return Ok(_stockService.GetById(User.GetCuit(), branchId, productId));
    }

    // Da de alta un producto en la sucursal (si estaba dado de baja, se reactiva)
    [Authorize(Roles = "Owner")]
    [HttpPost]
    public ActionResult<BranchStock> Create(Guid branchId, BranchStock newStock)
    {
        BranchStock created = _stockService.Create(User.GetCuit(), branchId, newStock);
        return CreatedAtAction(nameof(GetById), new { branchId, productId = created.ProductId }, created);
    }

    [Authorize(Roles = "Owner")]
    [HttpPut("{productId}")]
    public IActionResult Update(Guid branchId, Guid productId, BranchStock updatedStock)
    {
        _stockService.Update(User.GetCuit(), branchId, productId, updatedStock);
        return NoContent();
    }

    // Baja lógica del producto en la sucursal
    [Authorize(Roles = "Owner")]
    [HttpDelete("{productId}")]
    public IActionResult Delete(Guid branchId, Guid productId)
    {
        _stockService.Delete(User.GetCuit(), branchId, productId);
        return NoContent();
    }
}
