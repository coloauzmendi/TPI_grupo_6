using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using web.Extensions;

namespace web.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class BranchController : ControllerBase
{
    private readonly IBranchRepository _branchRepository;
    private readonly IUserRepository _userRepository;
    private readonly AccessService _accessService;

    public BranchController(
        IBranchRepository branchRepository,
        IUserRepository userRepository,
        AccessService accessService)
    {
        _branchRepository = branchRepository;
        _userRepository = userRepository;
        _accessService = accessService;
    }

    // Dueño: las sucursales de sus empresas. Empleado: solo la suya.
    [HttpGet]
    public ActionResult<List<Branch>> GetAll()
    {
        return Ok(_accessService.GetBranches(User.GetCuit()));
    }

    [HttpGet("{id}")]
    public ActionResult<Branch> GetById(Guid id)
    {
        return Ok(_accessService.EnsureBranchAccess(User.GetCuit(), id));
    }

    [Authorize(Roles = "Owner")]
    [HttpPost]
    public ActionResult<Branch> Create(Branch newBranch)
    {
        _accessService.EnsureCompanyOwner(User.GetCuit(), newBranch.CompanyId);

        var created = _branchRepository.Create(newBranch);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Roles = "Owner")]
    [HttpPut("{id}")]
    public IActionResult Update(Guid id, Branch updatedBranch)
    {
        var branch = _accessService.EnsureBranchAccess(User.GetCuit(), id);
        _accessService.EnsureCompanyOwner(User.GetCuit(), updatedBranch.CompanyId);

        branch.Locality = updatedBranch.Locality;
        branch.Address = updatedBranch.Address;
        branch.Industry = updatedBranch.Industry;
        branch.CompanyId = updatedBranch.CompanyId;
        _branchRepository.Update(branch);

        return NoContent();
    }

    // Baja lógica: también se dan de baja los empleados asignados a la sucursal
    [Authorize(Roles = "Owner")]
    [HttpDelete("{id}")]
    public IActionResult Delete(Guid id)
    {
        var branch = _accessService.EnsureBranchAccess(User.GetCuit(), id);

        foreach (var employee in _userRepository.GetByBranch(id))
            _userRepository.Delete(employee);

        _branchRepository.Delete(branch);
        return NoContent();
    }
}