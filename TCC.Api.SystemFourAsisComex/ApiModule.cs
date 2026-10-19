namespace TCC.Api.SystemFourAsisComex;

/// <summary>
/// Metadatos del módulo API. Permite que la API funcione de forma independiente
/// (su propio Program.cs) o integrada dentro de TCC.Web.InternalSystems sin
/// colisiones: cada módulo tiene su área, su esquema JWT y su carpeta de datos.
/// </summary>
public static class ApiModule
{
    /// <summary>Área de ruteo: los endpoints quedan en api/AsisComex/[controller].</summary>
    public const string Area = "AsisComex";

    /// <summary>Esquema JWT propio del módulo (evita "Scheme already exists: Bearer").</summary>
    public const string AuthScheme = "Bearer_AsisComex";

    /// <summary>
    /// Carpeta donde están el Excel y los JSON de configuración del módulo.
    /// Independiente: {bin}/Data. Integrada en la Web: {bin}/Data/AsisComex.
    /// Se asigna desde AddApplicationServices.
    /// </summary>
    public static string DataDirectory { get; private set; } = Path.Combine(AppContext.BaseDirectory, "Data");

    internal static void UseDataDirectory(string? dataDirectory)
    {
        if (!string.IsNullOrWhiteSpace(dataDirectory))
        {
            DataDirectory = dataDirectory;
        }
    }
}
