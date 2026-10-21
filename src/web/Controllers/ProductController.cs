using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace web.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductRepository _productRepository;

    public ProductController(IProductRepository productRepository)
    {
        _productRepository = productRepository;
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

    [HttpPost]
    public ActionResult<Product> Create(Product newProduct)
    {
        if (newProduct.Price < 0)
            return BadRequest("El precio debe ser mayor o igual a 0.");

        var created = _productRepository.Create(newProduct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

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

    [HttpDelete("{id}")]
    public IActionResult Delete(Guid id)
    {
        var product = _productRepository.GetById(id);
        if (product is null)
            return NotFound();

        _productRepository.Delete(product);
        return NoContent();
    }
}
