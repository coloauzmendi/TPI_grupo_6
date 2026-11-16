using Application.Models;
using Application.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using web.Extensions;

namespace web.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly ProductService _productService;
    private readonly PriceService _priceService;

    public ProductController(ProductService productService, PriceService priceService)
    {
        _productService = productService;
        _priceService = priceService;
    }

    [HttpGet]
    public ActionResult<List<Product>> GetAll()
    {
        return Ok(_productService.GetAll());
    }

    [HttpGet("{id}")]
    public ActionResult<Product> GetById(Guid id)
    {
        return Ok(_productService.GetById(id));
    }

    [Authorize(Roles = "Owner")]
    [HttpPost]
    public ActionResult<Product> Create(Product newProduct)
    {
        Product created = _productService.Create(newProduct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // Solo productos cargados en alguna sucursal del dueño
    [Authorize(Roles = "Owner")]
    [HttpPut("{id}")]
    public IActionResult Update(Guid id, Product updatedProduct)
    {
        _productService.Update(User.GetCuit(), id, updatedProduct);
        return NoContent();
    }

    // Solo si el producto no tiene ventas ni compras registradas
    [Authorize(Roles = "Owner")]
    [HttpDelete("{id}")]
    public IActionResult Delete(Guid id)
    {
        _productService.Delete(User.GetCuit(), id);
        return NoContent();
    }

    // Actualización masiva de precios; devuelve cuántos productos cambiaron
    [Authorize(Roles = "Owner")]
    [HttpPost("prices")]
    public ActionResult<int> UpdatePrices(PriceUpdateRequest request)
    {
        return Ok(_priceService.Update(User.GetCuit(), request));
    }
}
