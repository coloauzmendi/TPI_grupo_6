using Application.Interfaces;
using Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace web.Controllers;

[ApiController]
[Route("api/authentication")]
public class AuthenticationController : ControllerBase
{
    private readonly ICustomAuthenticationService _authenticationService;

    public AuthenticationController(ICustomAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    // Es un POST porque las credenciales viajan en el body
    [HttpPost("authenticate")]
    public ActionResult<string> Authenticate(AuthenticationRequest request)
    {
        string token = _authenticationService.Authenticate(request);
        return Ok(token);
    }
}
