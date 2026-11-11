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
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly PriceService _priceService;

    public ProductController(IProductRepository productRepository, PriceService priceService)
    {
        _productRepository = productRepository;
        _priceService = priceService;
    }

    [HttpGet]
    public ActionResult<List<Product>> GetAll()
    {
        return Ok(_productRepository.GetAll());
    }

    [HttpGet("{id}")]
    public ActionResult<Product> GetById(Guid id)
    {
        var product = _productRepository.GetById(id);
        if (product is null)
            return NotFound();

        return Ok(product);
    }

    [Authorize(Roles = "Owner")]
    [HttpPost]
    public ActionResult<Product> Create(Product newProduct)
    {
        if (newProduct.Price < 0)
            return BadRequest("El precio debe ser mayor o igual a 0.");

        var created = _productRepository.Create(newProduct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Roles = "Owner")]
    [HttpPut("{id}")]
    public IActionResult Update(Guid id, Product updatedProduct)
    {
        var product = _productRepository.GetById(id);
        if (product is null)
            return NotFound();

        if (updatedProduct.Price < 0)
            return BadRequest("El precio debe ser mayor o igual a 0.");

        product.Name = updatedProduct.Name;
        product.Brand = updatedProduct.Brand;
        product.Category = updatedProduct.Category;
        product.Price = updatedProduct.Price;
        product.Industry = updatedProduct.Industry;
        _productRepository.Update(product);

        return NoContent();
    }

    [Authorize(Roles = "Owner")]
    [HttpDelete("{id}")]
    public IActionResult Delete(Guid id)
    {
        var product = _productRepository.GetById(id);
        if (product is null)
            return NotFound();

        _productRepository.Delete(product);
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