using System.Text;
using System.Text.Json.Serialization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Infraestructure.Data;
using Infraestructure.Repositories;
using Infraestructure.Services;
using Application.Interfaces;
using Application.Services;
using web.Handlers;
using static Infraestructure.Services.AuthenticationService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Los enums (Role, Industry, PriceMode) viajan como texto en el JSON: "Owner", "Kiosk", etc. No se aceptan números.
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)));
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

// Valores obligatorios: en desarrollo salen de appsettings.Development.json y en producción de
// variables de entorno (ej. ConnectionStrings__DefaultConnection, AuthenticationService__SecretForKey)
string RequiredSetting(string key) =>
    string.IsNullOrWhiteSpace(builder.Configuration[key])
        ? throw new InvalidOperationException($"Falta la configuración '{key}' (variable de entorno '{key.Replace(":", "__")}').")
        : builder.Configuration[key]!;

string connectionString = RequiredSetting("ConnectionStrings:DefaultConnection");
string jwtIssuer = RequiredSetting("AuthenticationService:Issuer");
string jwtAudience = RequiredSetting("AuthenticationService:Audience");
string jwtSecret = RequiredSetting("AuthenticationService:SecretForKey");

// HMAC-SHA256 exige una clave de al menos 256 bits; si no, el error aparecería recién al primer login
if (Encoding.ASCII.GetByteCount(jwtSecret) < 32)
{
    throw new InvalidOperationException("AuthenticationService:SecretForKey debe tener al menos 32 caracteres.");
}

// Agregar el ApplicationContext al contenedor de servicios.
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
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret))
        };
    });

builder.Services.Configure<AuthenticationServiceOptions>(
    builder.Configuration.GetSection(AuthenticationServiceOptions.AuthenticationService));

// El FrontEnd (Vite) corre en otro origen, así que hay que permitirlo.
// Cors:AllowedOrigins puede ser un arreglo en appsettings o, como variable de entorno,
// una lista separada por comas (Cors__AllowedOrigins=https://a.com,https://b.com)
IConfigurationSection corsSection = builder.Configuration.GetSection("Cors:AllowedOrigins");
string[] allowedOrigins = corsSection.GetChildren().Any()
    ? corsSection.Get<string[]>()!
    : (corsSection.Value ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
if (allowedOrigins.Length == 0)
{
    allowedOrigins = ["http://localhost:5173"];
}

const string FrontendCorsPolicy = "Frontend";
builder.Services.AddCors(options =>
    options.AddPolicy(FrontendCorsPolicy, policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()));

// Servicio de terceros: inflación (IPC del INDEC) desde datos.gob.ar, con HttpClient creado por IHttpClientFactory
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<IInflationService, InflationService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalApis:SeriesBaseUrl"] ?? "https://apis.datos.gob.ar/series/api/");
    client.Timeout = TimeSpan.FromSeconds(5);
});

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
builder.Services.AddScoped<BranchService>();
builder.Services.AddScoped<StockService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IInflationRateService, InflationRateService>();
builder.Services.AddScoped<SaleService>();
builder.Services.AddScoped<PurchaseService>();
builder.Services.AddScoped<PriceService>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<ICustomAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IPasswordHashService, PasswordHashService>();

// Manejo global de errores: toda excepción no capturada se devuelve como ProblemDetails
builder.Services.AddExceptionHandler<AppExceptionHandler>();
builder.Services.AddProblemDetails(options =>
{
    // Agrega el traceId a cada error para poder buscarlo en los logs
    options.CustomizeProblemDetails = context =>
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
});

var app = builder.Build();

// Inicialización de la base al arrancar (no se ejecuta durante "dotnet ef", que se detiene en Build)
// 1) SQLite crea el archivo pero no la carpeta: en Azure App Service Linux la base vive en /home/data,
//    que es el único almacenamiento que persiste entre reinicios y despliegues.
string? databaseFolder = Path.GetDirectoryName(new SqliteConnectionStringBuilder(connectionString).DataSource);
if (!string.IsNullOrEmpty(databaseFolder))
{
    Directory.CreateDirectory(databaseFolder);
}

// 2) El journal_mode se configura una sola vez, con una conexión aparte. Se usa DELETE y no WAL
//    porque /home en App Service es almacenamiento de red, donde WAL no es confiable.
using (var setupConnection = new SqliteConnection(connectionString))
{
    setupConnection.Open();
    using var command = setupConnection.CreateCommand();
    command.CommandText = "PRAGMA journal_mode = DELETE;";
    command.ExecuteNonQuery();
}

// 3) Se aplican las migraciones pendientes (crea la base completa si es nueva)
using (IServiceScope scope = app.Services.CreateScope())
{
    ApplicationContext db = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
    db.Database.Migrate();
}

app.UseExceptionHandler();
// Los 401/403 que genera [Authorize] también se devuelven como ProblemDetails
app.UseStatusCodePages();

// Configure the HTTP request pipeline.
// Swagger siempre en Development; en Azure se activa con la variable de entorno Swagger__Enabled=true
if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Swagger:Enabled"))
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
