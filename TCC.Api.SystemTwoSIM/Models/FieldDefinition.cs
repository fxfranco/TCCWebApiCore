namespace TCC.Api.SystemTwoSIM.Models;

/// <summary>
/// Representa un campo a extraer del archivo Excel, definido en el JSON de configuración.
/// </summary>
public class FieldDefinition
{
    /// <summary>Nombre lógico/visible del campo.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Nombre de la columna esperada en el encabezado del Excel.</summary>
    public string Column { get; set; } = string.Empty;

    /// <summary>
    /// Tipo del campo: "text" (por defecto) o "date". Los campos "date" se entregan
    /// siempre como "yyyy-MM-dd" (ver IDateValueNormalizer).
    /// </summary>
    public string Type { get; set; } = "text";

    public bool IsDate => string.Equals(Type, "date", StringComparison.OrdinalIgnoreCase);
}

/// <summary>
/// Contenedor raíz del archivo JSON de configuración de campos.
/// </summary>
public class FieldDefinitionsConfig
{
    public List<FieldDefinition> Fields { get; set; } = new();
}
