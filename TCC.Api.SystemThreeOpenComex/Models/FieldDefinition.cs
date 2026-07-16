namespace TCC.Api.SystemThreeOpenComex.Models;

/// <summary>
/// Representa un campo a extraer del archivo Excel, definido en el JSON de configuración.
/// </summary>
public class FieldDefinition
{
    /// <summary>Nombre lógico/visible del campo.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Nombre de la columna esperada en el encabezado del Excel.</summary>
    public string Column { get; set; } = string.Empty;
}

/// <summary>
/// Contenedor raíz del archivo JSON de configuración de campos.
/// </summary>
public class FieldDefinitionsConfig
{
    public List<FieldDefinition> Fields { get; set; } = new();
}
