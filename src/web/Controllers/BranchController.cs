using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace web.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class BranchController : ControllerBase
{
    private readonly IBranchRepository _branchRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IUserRepository _userRepository;

    public BranchController(
        IBranchRepository branchRepository,
        ICompanyRepository companyRepository,
        IUserRepository userRepository)
    {
        _branchRepository = branchRepository;
        _companyRepository = companyRepository;
        _userRepository = userRepository;
    }

    [HttpGet]
    public ActionResult<List<Branch>> GetAll()
    {
        return Ok(_branchRepository.GetAll());
    }

    [HttpGet("{id}")]
    public ActionResult<Branch> GetById(Guid id)
    {
        var branch = _branchRepository.GetById(id);
        if (branch is null)
            return NotFound();

        return Ok(branch);
    }

    [HttpPost]
    public ActionResult<Branch> Create(Branch newBranch)
    {
        if (_companyRepository.GetById(newBranch.CompanyId) is null)
            return BadRequest("La empresa indicada no existe.");

        var created = _branchRepository.Create(newBranch);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public IActionResult Update(Guid id, Branch updatedBranch)
    {
        var branch = _branchRepository.GetById(id);
        if (branch is null)
            return NotFound();

        if (_companyRepository.GetById(updatedBranch.CompanyId) is null)
            return BadRequest("La empresa indicada no existe.");

        branch.Locality = updatedBranch.Locality;
        branch.Address = updatedBranch.Address;
        branch.Industry = updatedBranch.Industry;
        branch.CompanyId = updatedBranch.CompanyId;
        _branchRepository.Update(branch);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(Guid id)
    {
        var branch = _branchRepository.GetById(id);
        if (branch is null)
            return NotFound();

        if (_userRepository.GetByBranch(id).Count > 0)
            return Conflict("La sucursal tiene empleados asignados.");

        _branchRepository.Delete(branch);
        return NoContent();
    }
}
