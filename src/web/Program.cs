using System.Text.Json.Serialization;
using web.Extensions;
using web.Handlers;

var builder = WebApplication.CreateBuilder(args);

builder.UsePortFromEnvironment();

// Los enums (Role, Industry, PriceMode) viajan como texto en el JSON: "Owner", "Kiosk", etc. No se aceptan números.
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)));

builder.Services.AddSwaggerWithJwt();
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddFrontendCors(builder.Configuration);
builder.Services.AddReverseProxySupport();
builder.Services.AddApiHealthChecks();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddRepositories();
builder.Services.AddApplicationServices();

// Manejo global de errores: toda excepción no capturada se devuelve como ProblemDetails
builder.Services.AddExceptionHandler<AppExceptionHandler>();
builder.Services.AddProblemDetails(options =>
{
    // Agrega el traceId a cada error para poder buscarlo en los logs
    options.CustomizeProblemDetails = context =>
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
});

var app = builder.Build();

app.InitializeDatabase();

app.UseReverseProxySupport();

app.UseExceptionHandler();
// Los 401/403 que genera [Authorize] también se devuelven como ProblemDetails
app.UseStatusCodePages();

// Swagger siempre en Development; en producción se activa con la variable de entorno Swagger__Enabled=true
if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseProxyAwareHttpsRedirection();

app.UseFrontendCors();

// Primero se autentica (se valida el JWT) y después se autoriza
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapApiHealthChecks();

app.Run();
