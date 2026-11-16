namespace web.Extensions;

public static class HostingExtensions
{
    private const string PortKey = "PORT";

    public static WebApplicationBuilder UsePortFromEnvironment(this WebApplicationBuilder builder)
    {
        string? port = builder.Configuration[PortKey];
        if (string.IsNullOrWhiteSpace(port))
        {
            return builder;
        }

        if (!int.TryParse(port, out int portNumber) || portNumber is < 1 or > 65535)
        {
            throw new InvalidOperationException($"La variable de entorno '{PortKey}' no es un puerto válido: '{port}'.");
        }

        builder.WebHost.UseUrls($"http://0.0.0.0:{portNumber}");

        return builder;
    }
}
