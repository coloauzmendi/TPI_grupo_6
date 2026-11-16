namespace web.Extensions;

public static class ConfigurationExtensions
{
    // Valores obligatorios: en desarrollo salen de appsettings.Development.json y en producción de
    // variables de entorno (ej. ConnectionStrings__DefaultConnection, AuthenticationService__SecretForKey)
    public static string GetRequiredSetting(this IConfiguration configuration, string key)
    {
        return string.IsNullOrWhiteSpace(configuration[key])
            ? throw new InvalidOperationException($"Falta la configuración '{key}' (variable de entorno '{key.Replace(":", "__")}').")
            : configuration[key]!;
    }
}
