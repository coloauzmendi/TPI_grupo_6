using Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace web.Handlers;

// Manejo global de errores: cualquier excepción no capturada en la API termina acá
// y se devuelve al cliente como ProblemDetails (RFC 7807).
public class AppExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<AppExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public AppExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<AppExceptionHandler> logger,
        IHostEnvironment environment)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
            AppValidationException => (StatusCodes.Status400BadRequest, "Solicitud inválida"),
            UnauthorizedException => (StatusCodes.Status401Unauthorized, "No autorizado"),
            NotAllowedException => (StatusCodes.Status403Forbidden, "Acción no permitida"),
            BadHttpRequestException badRequest => (badRequest.StatusCode, "Solicitud inválida"),
            _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor")
        };

        string detail;
        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            // Error inesperado: se registra completo y no se exponen detalles internos fuera de Development
            _logger.LogError(exception, "Error no controlado en {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
            detail = _environment.IsDevelopment()
                ? exception.ToString()
                : "Ocurrió un error inesperado. Intente nuevamente más tarde.";
        }
        else
        {
            _logger.LogWarning("{ExceptionType}: {Message}", exception.GetType().Name, exception.Message);
            detail = exception.Message;
        }

        httpContext.Response.StatusCode = statusCode;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}"
            }
        });
    }
}
