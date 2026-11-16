using Application.Models;
using Application.Services;
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
    public ActionResult<List<ProductResponse>> GetAll()
    {
        return Ok(_productService.GetAll());
    }

    [HttpGet("{id}")]
    public ActionResult<ProductResponse> GetById(Guid id)
    {
        return Ok(_productService.GetById(id));
    }

    [Authorize(Roles = "Owner")]
    [HttpPost]
    public ActionResult<ProductResponse> Create(ProductRequest request)
    {
        ProductResponse created = _productService.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Roles = "Owner")]
    [HttpPut("{id}")]
    public IActionResult Update(Guid id, ProductRequest request)
    {
        _productService.Update(id, request);
        return NoContent();
    }

    [Authorize(Roles = "Owner")]
    [HttpDelete("{id}")]
    public IActionResult Delete(Guid id)
    {
        _productService.Delete(id);
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
