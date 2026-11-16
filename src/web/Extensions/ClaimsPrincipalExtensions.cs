using System.Security.Claims;
using Domain.Exceptions;

namespace web.Extensions;

public static class ClaimsPrincipalExtensions
{
    // CUIT del usuario autenticado: viaja en el claim "sub" del token
    public static string GetCuit(this ClaimsPrincipal principal)
    {
        return principal.FindFirst("sub")?.Value
            ?? throw new UnauthorizedException("El token no contiene el CUIT del usuario.");
    }

    // Igual que GetCuit, pero null si la petición no viene autenticada (endpoints anónimos)
    public static string? GetCuitOrNull(this ClaimsPrincipal principal)
    {
        return principal.Identity?.IsAuthenticated == true ? principal.GetCuit() : null;
    }
}