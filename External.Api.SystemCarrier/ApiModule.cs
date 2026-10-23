namespace External.Api.SystemCarrier;

/// <summary>
/// Metadatos del módulo API. Permite que la API funcione de forma independiente
/// (su propio Program.cs) o integrada dentro de TCC.Web.InternalSystems sin
/// colisiones: cada módulo tiene su área, su esquema JWT y su carpeta de datos.
/// </summary>
public static class ApiModule
{
    /// <summary>Área de ruteo: los endpoints quedan en api/Carrier/[controller].</summary>
    public const string Area = "Carrier";

    /// <summary>Esquema JWT propio del módulo (evita "Scheme already exists: Bearer").</summary>
    public const string AuthScheme = "Bearer_Carrier";

    /// <summary>
    /// Campo de auditoría que se agrega SIEMPRE a cada registro (aunque no venga en el Excel ni en
    /// fields-config.json). Vacío por defecto; la Web lo llena con el usuario que agrega/modifica.
    /// Al guardar (ApplicationDatabase_N.xlsx) se escribe como una columna más.
    /// </summary>
    public const string ChangeUserField = "changeuser";

    /// <summary>
    /// Carpeta donde están el Excel y los JSON de configuración del módulo.
    /// Independiente: {bin}/Data. Integrada en la Web: {bin}/Data/Carrier.
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
