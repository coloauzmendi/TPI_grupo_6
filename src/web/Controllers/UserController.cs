using Application.Interfaces;
using Application.Models;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace web.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserController(IUserRepository userRepository, IBranchRepository branchRepository, IPasswordHasher<User> passwordHasher)
    {
        _userRepository = userRepository;
        _branchRepository = branchRepository;
        _passwordHasher = passwordHasher;
    }

    [HttpGet]
    public ActionResult<List<User>> GetAll()
    {
        return Ok(_userRepository.GetAll());
    }

    [HttpGet("{cuit}")]
    public ActionResult<User> GetByCuit(string cuit)
    {
        var user = _userRepository.GetByCuit(cuit);
        if (user is null)
            return NotFound();

        return Ok(user);
    }

    // Público: sin esto nadie podría crear el primer usuario para loguearse
    [AllowAnonymous]
    [HttpPost]
    public ActionResult<User> Create(UserCreateRequest request)
    {
        if (_userRepository.GetByCuit(request.Cuit) is not null)
            return Conflict("Ya existe un usuario con ese CUIT.");

        if (string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("La contraseña es obligatoria.");

        var newUser = new User
        {
            Cuit = request.Cuit,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = request.Role,
            BranchId = request.BranchId
        };

        var error = ValidateRoleAndBranch(newUser);
        if (error is not null)
            return BadRequest(error);

        // Se guarda solo el hash, nunca la contraseña en texto plano
        newUser.PasswordHash = _passwordHasher.HashPassword(newUser, request.Password);

        var created = _userRepository.Create(newUser);
        return CreatedAtAction(nameof(GetByCuit), new { cuit = created.Cuit }, created);
    }

    [HttpPut("{cuit}")]
    public IActionResult Update(string cuit, User updatedUser)
    {
        var user = _userRepository.GetByCuit(cuit);
        if (user is null)
            return NotFound();

        var error = ValidateRoleAndBranch(updatedUser);
        if (error is not null)
            return BadRequest(error);

        user.FirstName = updatedUser.FirstName;
        user.LastName = updatedUser.LastName;
        user.Role = updatedUser.Role;
        user.BranchId = updatedUser.BranchId;
        _userRepository.Update(user);

        return NoContent();
    }

    [HttpDelete("{cuit}")]
    public IActionResult Delete(string cuit)
    {
        var user = _userRepository.GetByCuit(cuit);
        if (user is null)
            return NotFound();

        _userRepository.Delete(user);
        return NoContent();
    }

    // Employee debe tener una sucursal existente; Owner no tiene sucursal
    private string? ValidateRoleAndBranch(User user)
    {
        if (user.Role == UserRole.Owner && user.BranchId is not null)
            return "Un usuario Owner no puede tener sucursal.";

        if (user.Role == UserRole.Employee)
        {
            if (user.BranchId is null)
                return "Un usuario Employee debe tener una sucursal asignada.";

            if (_branchRepository.GetById(user.BranchId.Value) is null)
                return "La sucursal indicada no existe.";
        }

        return null;
    }
}
