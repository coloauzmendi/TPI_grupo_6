using Application.Models;
using Application.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using web.Extensions;

namespace web.Controllers;

[ApiController]
[Authorize(Roles = "Owner")]
[Route("api/[controller]")]
public class CompanyController : ControllerBase
{
    private readonly CompanyService _companyService;
    private readonly AccessService _accessService;

    public CompanyController(CompanyService companyService, AccessService accessService)
    {
        _companyService = companyService;
        _accessService = accessService;
    }

    // Solo las empresas de las que el usuario es dueño
    [HttpGet]
    public ActionResult<List<Company>> GetAll()
    {
        return Ok(_accessService.GetCompanies(User.GetCuit()));
    }

    [HttpGet("{id}")]
    public ActionResult<Company> GetById(Guid id)
    {
        return Ok(_accessService.EnsureCompanyOwner(User.GetCuit(), id));
    }

    [HttpPost]
    public ActionResult<Company> Create(CompanyRequest request)
    {
        var created = _companyService.Create(request, User.GetCuit());
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public IActionResult Update(Guid id, CompanyRequest request)
    {
        _accessService.EnsureCompanyOwner(User.GetCuit(), id);
        _companyService.Update(id, request);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(Guid id)
    {
        _accessService.EnsureCompanyOwner(User.GetCuit(), id);
        _companyService.Delete(id);
        return NoContent();
    }

    // Dueños de la empresa

    [HttpGet("{id}/owners")]
    public ActionResult<List<Domain.Entities.User>> GetOwners(Guid id)
    {
        _accessService.EnsureCompanyOwner(User.GetCuit(), id);
        return Ok(_companyService.GetOwners(id));
    }

    [HttpPost("{id}/owners/{cuit}")]
    public IActionResult AddOwner(Guid id, string cuit)
    {
        _accessService.EnsureCompanyOwner(User.GetCuit(), id);
        _companyService.AddOwner(id, cuit);
        return NoContent();
    }

    [HttpDelete("{id}/owners/{cuit}")]
    public IActionResult RemoveOwner(Guid id, string cuit)
    {
        _accessService.EnsureCompanyOwner(User.GetCuit(), id);
        _companyService.RemoveOwner(id, cuit);
        return NoContent();
    }
}