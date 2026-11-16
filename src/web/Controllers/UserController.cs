using Application.Interfaces;
using Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using web.Extensions;

namespace web.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    // Empleados de las sucursales del dueño
    [Authorize(Roles = "Owner")]
    [HttpGet]
    public ActionResult<List<UserResponse>> GetAll()
    {
        return Ok(_userService.GetEmployees(User.GetCuit()));
    }

    [HttpGet("{cuit}")]
    public ActionResult<UserResponse> GetByCuit(string cuit)
    {
        return Ok(_userService.GetByCuit(User.GetCuit(), cuit));
    }

    // Público solo para registrar un dueño: sin esto nadie podría crear el primer usuario para loguearse.
    // Crear un empleado exige estar logueado como dueño de su sucursal.
    [AllowAnonymous]
    [HttpPost]
    public ActionResult<UserResponse> Create(UserCreateRequest request)
    {
        UserResponse created = _userService.Create(User.GetCuitOrNull(), request);
        return CreatedAtAction(nameof(GetByCuit), new { cuit = created.Cuit }, created);
    }

    // Edita un empleado de una sucursal del dueño; la contraseña solo se cambia si viene en el body
    [Authorize(Roles = "Owner")]
    [HttpPut("{cuit}")]
    public IActionResult Update(string cuit, UserUpdateRequest request)
    {
        _userService.Update(User.GetCuit(), cuit, request);
        return NoContent();
    }

    // Baja lógica
    [Authorize(Roles = "Owner")]
    [HttpDelete("{cuit}")]
    public IActionResult Delete(string cuit)
    {
        _userService.Delete(User.GetCuit(), cuit);
        return NoContent();
    }
}
