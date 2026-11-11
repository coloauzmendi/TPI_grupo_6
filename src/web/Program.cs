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

// El journal_mode se configura una sola vez al arrancar, con una conexión aparte
using (var setupConnection = new SqliteConnection(connectionString))
{
    setupConnection.Open();
    using var command = setupConnection.CreateCommand();
    command.CommandText = "PRAGMA journal_mode = DELETE;";
    command.ExecuteNonQuery();
}

// Cada DbContext (uno por request) abre su propia conexión: compartir una sola entre requests
// simultáneas provoca "SQLite Error 5: unable to delete/modify user-function due to active statements"
builder.Services.AddDbContext<ApplicationContext>(dbContextOptions => dbContextOptions.UseSqlite(connectionString));

// Autenticación con JWT: qué se verifica del token que viene en cada request y contra qué valores
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        // Se conservan los nombres de los claims del token ("sub", "role") en vez de mapearlos a los de Microsoft
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new()
        {
            NameClaimType = "sub",
            RoleClaimType = "role", // permite usar [Authorize(Roles = "Owner")]
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

// El FrontEnd (Vite) corre en otro origen, así que hay que permitirlo
const string FrontendCorsPolicy = "Frontend";
builder.Services.AddCors(options =>
    options.AddPolicy(FrontendCorsPolicy, policy => policy
        .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:5173"])
        .AllowAnyHeader()
        .AllowAnyMethod()));

// Configurar repositorios como Scoped
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IBranchRepository, BranchRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IBranchStockRepository, BranchStockRepository>();
builder.Services.AddScoped<ISaleRepository, SaleRepository>();
builder.Services.AddScoped<IPurchaseRepository, PurchaseRepository>();

// Configurar servicios como Scoped
builder.Services.AddScoped<CompanyService>();
builder.Services.AddScoped<AccessService>();
builder.Services.AddScoped<MeService>();
builder.Services.AddScoped<StockService>();
builder.Services.AddScoped<SaleService>();
builder.Services.AddScoped<PurchaseService>();
builder.Services.AddScoped<PriceService>();
builder.Services.AddScoped<ReportService>();
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

app.UseCors(FrontendCorsPolicy);

// Primero se autentica (se valida el JWT) y después se autoriza
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
