using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using web.Extensions;

namespace web.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class BranchController : ControllerBase
{
    private readonly BranchService _branchService;

    public BranchController(BranchService branchService)
    {
        _branchService = branchService;
    }

    // Dueño: las sucursales de sus empresas. Empleado: solo la suya.
    [HttpGet]
    public ActionResult<List<BranchResponse>> GetAll()
    {
        return Ok(_branchService.GetAll(User.GetCuit()));
    }

    [HttpGet("{id}")]
    public ActionResult<BranchResponse> GetById(Guid id)
    {
        return Ok(_branchService.GetById(User.GetCuit(), id));
    }

    [Authorize(Roles = "Owner")]
    [HttpPost]
    public ActionResult<BranchResponse> Create(BranchRequest request)
    {
        BranchResponse created = _branchService.Create(User.GetCuit(), request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Roles = "Owner")]
    [HttpPut("{id}")]
    public IActionResult Update(Guid id, BranchRequest request)
    {
        _branchService.Update(User.GetCuit(), id, request);
        return NoContent();
    }

    // Baja lógica: también se dan de baja los empleados asignados a la sucursal
    [Authorize(Roles = "Owner")]
    [HttpDelete("{id}")]
    public IActionResult Delete(Guid id)
    {
        _branchService.Delete(User.GetCuit(), id);
        return NoContent();
    }
}
