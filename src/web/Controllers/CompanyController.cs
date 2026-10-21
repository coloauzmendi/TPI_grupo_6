using Application.Models;
using Application.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace web.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CompanyController : ControllerBase
{
    private readonly CompanyService _companyService;

    public CompanyController(CompanyService companyService)
    {
        _companyService = companyService;
    }

    [HttpGet]
    public ActionResult<List<Company>> GetAll()
    {
        return Ok(_companyService.GetAll());
    }

    [HttpGet("{id}")]
    public ActionResult<Company> GetById(Guid id)
    {
        return Ok(_companyService.GetById(id));
    }

    [HttpPost]
    public ActionResult<Company> Create(CompanyRequest request)
    {
        var created = _companyService.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public IActionResult Update(Guid id, CompanyRequest request)
    {
        _companyService.Update(id, request);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(Guid id)
    {
        _companyService.Delete(id);
        return NoContent();
    }

    // Dueños de la empresa

    [HttpGet("{id}/owners")]
    public ActionResult<List<User>> GetOwners(Guid id)
    {
        return Ok(_companyService.GetOwners(id));
    }

    [HttpPost("{id}/owners/{cuit}")]
    public IActionResult AddOwner(Guid id, string cuit)
    {
        _companyService.AddOwner(id, cuit);
        return NoContent();
    }

    [HttpDelete("{id}/owners/{cuit}")]
    public IActionResult RemoveOwner(Guid id, string cuit)
    {
        _companyService.RemoveOwner(id, cuit);
        return NoContent();
    }
}
