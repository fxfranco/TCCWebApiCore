using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace TCC.Web.InternalSystems.Infrastructure;

/// <summary>
/// Resuelve la URL base con la que las páginas Blazor llaman a las APIs integradas
/// (que ahora se ejecutan dentro de esta misma aplicación).
///
/// Orden de resolución:
///   1. Configuración "InternalApi:BaseUrl" (variable de entorno InternalApi__BaseUrl).
///   2. Direcciones en las que Kestrel está escuchando (http://+:8080 en Docker,
///      https://localhost:7157 en Visual Studio, etc.), normalizadas a localhost.
/// </summary>
public sealed class InternalApiBaseAddress
{
    private readonly IConfiguration _configuration;
    private readonly IServer _server;
    private Uri? _cached;

    public InternalApiBaseAddress(IConfiguration configuration, IServer server)
    {
        _configuration = configuration;
        _server = server;
    }

    public Uri Get()
    {
        if (_cached is not null)
        {
            return _cached;
        }

        var configured = _configuration["InternalApi:BaseUrl"];
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return _cached = EnsureTrailingSlash(new Uri(configured));
        }

        var addresses = _server.Features.Get<IServerAddressesFeature>()?.Addresses ?? Array.Empty<string>();

        // Preferir https (evita la redirección de UseHttpsRedirection); si no existe, http.
        var address = addresses.FirstOrDefault(a => a.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                      ?? addresses.FirstOrDefault(a => a.StartsWith("http://", StringComparison.OrdinalIgnoreCase));

        if (address is null)
        {
            throw new InvalidOperationException(
                "No fue posible determinar la URL interna de la aplicación. Configure 'InternalApi:BaseUrl' " +
                "(por ejemplo http://localhost:8080/).");
        }

        // "http://+:8080", "http://*:8080", "http://[::]:8080", "http://0.0.0.0:8080" -> "http://localhost:8080"
        var normalized = address
            .Replace("://+", "://localhost")
            .Replace("://*", "://localhost")
            .Replace("://[::]", "://localhost")
            .Replace("://0.0.0.0", "://localhost");

        return _cached = EnsureTrailingSlash(new Uri(normalized));
    }

    private static Uri EnsureTrailingSlash(Uri uri) =>
        uri.AbsoluteUri.EndsWith('/') ? uri : new Uri(uri.AbsoluteUri + "/");
}
