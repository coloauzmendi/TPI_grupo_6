using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace web.Controllers;

[ApiController]
[Authorize]
[Route("api/branch/{branchId}/stock")]
public class BranchStockController : ControllerBase
{
    private readonly IBranchStockRepository _stockRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IProductRepository _productRepository;

    public BranchStockController(
        IBranchStockRepository stockRepository,
        IBranchRepository branchRepository,
        IProductRepository productRepository)
    {
        _stockRepository = stockRepository;
        _branchRepository = branchRepository;
        _productRepository = productRepository;
    }

    [HttpGet]
    public ActionResult<List<BranchStock>> GetByBranch(Guid branchId)
    {
        if (_branchRepository.GetById(branchId) is null)
            return NotFound();

        return Ok(_stockRepository.GetByBranch(branchId));
    }

    [HttpGet("{productId}")]
    public ActionResult<BranchStock> GetById(Guid branchId, Guid productId)
    {
        var stock = _stockRepository.GetById(branchId, productId);
        if (stock is null)
            return NotFound();

        return Ok(stock);
    }

    [HttpPost]
    public ActionResult<BranchStock> Create(Guid branchId, BranchStock newStock)
    {
        newStock.BranchId = branchId;

        if (_branchRepository.GetById(branchId) is null)
            return NotFound("La sucursal no existe.");

        if (_productRepository.GetById(newStock.ProductId) is null)
            return BadRequest("El producto indicado no existe.");

        if (_stockRepository.GetById(branchId, newStock.ProductId) is not null)
            return Conflict("Ese producto ya tiene stock cargado en esta sucursal.");

        if (newStock.Quantity < 0 || newStock.MinimumStock < 0)
            return BadRequest("La cantidad y el stock mínimo no pueden ser negativos.");

        var created = _stockRepository.Create(newStock);
        return CreatedAtAction(nameof(GetById), new { branchId, productId = created.ProductId }, created);
    }

    [HttpPut("{productId}")]
    public IActionResult Update(Guid branchId, Guid productId, BranchStock updatedStock)
    {
        var stock = _stockRepository.GetById(branchId, productId);
        if (stock is null)
            return NotFound();

        if (updatedStock.Quantity < 0 || updatedStock.MinimumStock < 0)
            return BadRequest("La cantidad y el stock mínimo no pueden ser negativos.");

        stock.Quantity = updatedStock.Quantity;
        stock.MinimumStock = updatedStock.MinimumStock;
        _stockRepository.Update(stock);

        return NoContent();
    }

    [HttpDelete("{productId}")]
    public IActionResult Delete(Guid branchId, Guid productId)
    {
        var stock = _stockRepository.GetById(branchId, productId);
        if (stock is null)
            return NotFound();

        _stockRepository.Delete(stock);
        return NoContent();
    }
}
