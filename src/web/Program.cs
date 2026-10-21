using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Infraestructure.Data;
using Infraestructure.Repositories;
using Infraestructure.Services;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using web.Handlers;
using static Infraestructure.Services.AuthenticationService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Los enums (Role, Industry) viajan como texto en el JSON: "Owner", "Kiosk", etc.
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer(); 
builder.Services.AddSwaggerGen(options =>
{
    // Agrega el botón "Authorize" a Swagger para enviar el header Authorization: Bearer <token>
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Pegar acá el token generado al autenticarse."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

// Agregar el ApplicationContext al contenedor de servicios
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
var connection = new SqliteConnection(connectionString);
connection.Open();

using (var command = connection.CreateCommand())
{
    command.CommandText = "PRAGMA journal_mode = DELETE;";
    command.ExecuteNonQuery();
}

builder.Services.AddDbContext<ApplicationContext>(dbContextOptions => dbContextOptions.UseSqlite(connection));

// Autenticación con JWT: qué se verifica del token que viene en cada request y contra qué valores
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true, // la fecha de expiración se valida por defecto
            ValidIssuer = builder.Configuration["AuthenticationService:Issuer"],
            ValidAudience = builder.Configuration["AuthenticationService:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["AuthenticationService:SecretForKey"]!))
        };
    });

builder.Services.Configure<AuthenticationServiceOptions>(
    builder.Configuration.GetSection(AuthenticationServiceOptions.AuthenticationService));

// Configurar repositorios como Scoped
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IBranchRepository, BranchRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IBranchStockRepository, BranchStockRepository>();

// Configurar servicios como Scoped
builder.Services.AddScoped<CompanyService>();
builder.Services.AddScoped<ICustomAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Manejo global de errores: toda excepción no capturada se devuelve como ProblemDetails
builder.Services.AddExceptionHandler<AppExceptionHandler>();
builder.Services.AddProblemDetails(options =>
{
    // Agrega el traceId a cada error para poder buscarlo en los logs
    options.CustomizeProblemDetails = context =>
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
});

var app = builder.Build();

app.UseExceptionHandler();
// Los 401/403 que genera [Authorize] también se devuelven como ProblemDetails
app.UseStatusCodePages();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Primero se autentica (se valida el JWT) y después se autoriza
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
