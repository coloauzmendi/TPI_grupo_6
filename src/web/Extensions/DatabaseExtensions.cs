using Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace web.Extensions;

public static class DatabaseExtensions
{
    private const string ConnectionStringKey = "ConnectionStrings:DefaultConnection";
    private const string SeedEnabledKey = "Seed:Enabled";

    // Cada DbContext (uno por request) abre su propia conexión: compartir una sola entre requests
    // simultáneas provoca "SQLite Error 5: unable to delete/modify user-function due to active statements"
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetRequiredSetting(ConnectionStringKey);
        services.AddDbContext<ApplicationContext>(dbContextOptions => dbContextOptions.UseSqlite(connectionString));
        services.AddScoped<DatabaseSeeder>();

        return services;
    }

    // Inicialización de la base al arrancar (no se ejecuta durante "dotnet ef", que se detiene en Build)
    public static WebApplication InitializeDatabase(this WebApplication app)
    {
        string connectionString = app.Configuration.GetRequiredSetting(ConnectionStringKey);

        // 1) SQLite crea el archivo pero no la carpeta, así que se crea antes de abrir la conexión.
        string? databaseFolder = Path.GetDirectoryName(new SqliteConnectionStringBuilder(connectionString).DataSource);
        if (!string.IsNullOrEmpty(databaseFolder))
        {
            Directory.CreateDirectory(databaseFolder);
        }

        // 2) El journal_mode se configura una sola vez, con una conexión aparte.
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

        if (app.Configuration.GetValue<bool>(SeedEnabledKey))
        {
            using IServiceScope scope = app.Services.CreateScope();
            scope.ServiceProvider.GetRequiredService<DatabaseSeeder>().Seed();
        }

        return app;
    }
}
