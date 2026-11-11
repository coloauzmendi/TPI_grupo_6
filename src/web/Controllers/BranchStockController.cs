using Application.Interfaces;
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
    private readonly IBranchStockRepository _stockRepository;
    private readonly IProductRepository _productRepository;
    private readonly StockService _stockService;
    private readonly AccessService _accessService;

    public BranchStockController(
        IBranchStockRepository stockRepository,
        IProductRepository productRepository,
        StockService stockService,
        AccessService accessService)
    {
        _stockRepository = stockRepository;
        _productRepository = productRepository;
        _stockService = stockService;
        _accessService = accessService;
    }

    // Stock activo de la sucursal junto con los datos de cada producto
    [HttpGet]
    public ActionResult<List<StockItemResponse>> GetByBranch(Guid branchId)
    {
        _accessService.EnsureBranchAccess(User.GetCuit(), branchId);

        return Ok(_stockService.GetItems(branchId));
    }

    [HttpGet("{productId}")]
    public ActionResult<BranchStock> GetById(Guid branchId, Guid productId)
    {
        _accessService.EnsureBranchAccess(User.GetCuit(), branchId);

        var stock = _stockRepository.GetById(branchId, productId);
        if (stock is null || !stock.IsActive)
            return NotFound();

        return Ok(stock);
    }

    [Authorize(Roles = "Owner")]
    [HttpPost]
    public ActionResult<BranchStock> Create(Guid branchId, BranchStock newStock)
    {
        _accessService.EnsureBranchAccess(User.GetCuit(), branchId);
        newStock.BranchId = branchId;

        if (_productRepository.GetById(newStock.ProductId) is null)
            return BadRequest("El producto indicado no existe.");

        var existing = _stockRepository.GetById(branchId, newStock.ProductId);
        if (existing is not null && existing.IsActive)
            return Conflict("Ese producto ya tiene stock cargado en esta sucursal.");

        if (newStock.Quantity < 0 || newStock.MinimumStock < 0)
            return BadRequest("La cantidad y el stock mínimo no pueden ser negativos.");

        // Si el producto estaba dado de baja en la sucursal, se reactiva
        if (existing is not null)
        {
            existing.Quantity = newStock.Quantity;
            existing.MinimumStock = newStock.MinimumStock;
            existing.IsActive = true;
            _stockRepository.Update(existing);
            return CreatedAtAction(nameof(GetById), new { branchId, productId = existing.ProductId }, existing);
        }

        var created = _stockRepository.Create(newStock);
        return CreatedAtAction(nameof(GetById), new { branchId, productId = created.ProductId }, created);
    }

    [Authorize(Roles = "Owner")]
    [HttpPut("{productId}")]
    public IActionResult Update(Guid branchId, Guid productId, BranchStock updatedStock)
    {
        _accessService.EnsureBranchAccess(User.GetCuit(), branchId);

        var stock = _stockRepository.GetById(branchId, productId);
        if (stock is null || !stock.IsActive)
            return NotFound();

        if (updatedStock.Quantity < 0 || updatedStock.MinimumStock < 0)
            return BadRequest("La cantidad y el stock mínimo no pueden ser negativos.");

        stock.Quantity = updatedStock.Quantity;
        stock.MinimumStock = updatedStock.MinimumStock;
        _stockRepository.Update(stock);

        return NoContent();
    }

    [Authorize(Roles = "Owner")]
    [HttpDelete("{productId}")]
    public IActionResult Delete(Guid branchId, Guid productId)
    {
        _accessService.EnsureBranchAccess(User.GetCuit(), branchId);

        var stock = _stockRepository.GetById(branchId, productId);
        if (stock is null || !stock.IsActive)
            return NotFound();

        _stockRepository.Delete(stock);
        return NoContent();
    }
}