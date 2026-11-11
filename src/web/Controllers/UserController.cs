using System.Text.RegularExpressions;
using Application.Interfaces;
using Application.Models;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using web.Extensions;

namespace web.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private static readonly Regex CuitFormat = new(@"^\d{2}-\d{8}-\d$");

    private readonly IUserRepository _userRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly AccessService _accessService;

    public UserController(
        IUserRepository userRepository,
        IBranchRepository branchRepository,
        IPasswordHasher<User> passwordHasher,
        AccessService accessService)
    {
        _userRepository = userRepository;
        _branchRepository = branchRepository;
        _passwordHasher = passwordHasher;
        _accessService = accessService;
    }

    // Empleados de las sucursales del dueño
    [Authorize(Roles = "Owner")]
    [HttpGet]
    public ActionResult<List<User>> GetAll()
    {
        var users = _accessService.GetBranches(User.GetCuit())
            .SelectMany(b => _userRepository.GetByBranch(b.Id))
            .ToList();

        return Ok(users);
    }

    [HttpGet("{cuit}")]
    public ActionResult<User> GetByCuit(string cuit)
    {
        var user = _userRepository.GetByCuit(cuit);
        if (user is null || !user.IsActive)
            return NotFound();

        // Cada usuario ve su propio perfil; el dueño, además, el de sus empleados
        if (cuit != User.GetCuit())
        {
            if (!User.IsInRole("Owner") || user.BranchId is null)
                throw new NotAllowedException("No tenés acceso a ese usuario.");

            _accessService.EnsureBranchAccess(User.GetCuit(), user.BranchId.Value);
        }

        return Ok(user);
    }

    // Público solo para registrar un dueño: sin esto nadie podría crear el primer usuario para loguearse.
    // Crear un empleado exige estar logueado como dueño de su sucursal.
    [AllowAnonymous]
    [HttpPost]
    public ActionResult<User> Create(UserCreateRequest request)
    {
        if (!CuitFormat.IsMatch(request.Cuit))
            return BadRequest("El CUIT debe tener el formato 20-12345678-9.");

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

        if (newUser.Role == UserRole.Employee)
        {
            if (User.Identity?.IsAuthenticated != true)
                throw new UnauthorizedException("Iniciá sesión como dueño para crear empleados.");

            if (!User.IsInRole("Owner"))
                throw new NotAllowedException("Solo un dueño puede crear empleados.");

            _accessService.EnsureBranchAccess(User.GetCuit(), newUser.BranchId!.Value);
        }

        // Se guarda solo el hash, nunca la contraseña en texto plano
        newUser.PasswordHash = _passwordHasher.HashPassword(newUser, request.Password);

        var created = _userRepository.Create(newUser);
        return CreatedAtAction(nameof(GetByCuit), new { cuit = created.Cuit }, created);
    }

    // Edita un empleado de una sucursal del dueño; la contraseña solo se cambia si viene en el body
    [Authorize(Roles = "Owner")]
    [HttpPut("{cuit}")]
    public IActionResult Update(string cuit, UserUpdateRequest request)
    {
        var user = GetEmployeeOfOwner(cuit);

        if (request.BranchId is null)
            return BadRequest("Un usuario Employee debe tener una sucursal asignada.");

        _accessService.EnsureBranchAccess(User.GetCuit(), request.BranchId.Value);

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.BranchId = request.BranchId;
        if (!string.IsNullOrWhiteSpace(request.Password))
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _userRepository.Update(user);

        return NoContent();
    }

    // Baja lógica
    [Authorize(Roles = "Owner")]
    [HttpDelete("{cuit}")]
    public IActionResult Delete(string cuit)
    {
        var user = GetEmployeeOfOwner(cuit);

        _userRepository.Delete(user);
        return NoContent();
    }

    // Empleado activo asignado a una sucursal a la que el dueño tiene acceso
    private User GetEmployeeOfOwner(string cuit)
    {
        var user = _userRepository.GetByCuit(cuit);
        if (user is null || !user.IsActive || user.Role != UserRole.Employee)
            throw new NotFoundException("El empleado no existe.");

        _accessService.EnsureBranchAccess(User.GetCuit(), user.BranchId!.Value);

        return user;
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