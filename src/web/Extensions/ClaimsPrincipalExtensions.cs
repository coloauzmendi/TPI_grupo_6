using System.Security.Claims;
using Domain.Exceptions;

namespace web.Extensions;

public static class ClaimsPrincipalExtensions
{
    // CUIT del usuario autenticado: viaja en el claim "sub" del token
    public static string GetCuit(this ClaimsPrincipal principal)
    {
        return principal.GetCuitOrNull()
            ?? throw new UnauthorizedException("El token no contiene el CUIT del usuario.");
    }

    // Para endpoints públicos: null si la request no trae un token válido
    public static string? GetCuitOrNull(this ClaimsPrincipal principal)
    {
        return principal.Identity?.IsAuthenticated == true ? principal.FindFirst("sub")?.Value : null;
    }
}
