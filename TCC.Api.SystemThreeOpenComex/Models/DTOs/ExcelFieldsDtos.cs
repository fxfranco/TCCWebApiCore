using Microsoft.AspNetCore.Http;

namespace TCC.Api.SystemThreeOpenComex.Models.DTOs;

/// <summary>
/// Petición del endpoint de extracción: el archivo Excel y, opcionalmente,
/// el nombre del archivo JSON de configuración de campos a usar.
/// </summary>
public class ExcelFieldsRequest
{
    public IFormFile? ExcelFile { get; set; }

    /// <summary>
    /// Nombre del archivo JSON (dentro de la carpeta Data) con la lista de campos.
    /// Si no se especifica, se usa "fields-config.json".
    /// </summary>
    public string? FieldsConfigFileName { get; set; }
}

/// <summary>
/// Respuesta con los campos solicitados, las columnas no encontradas
/// en el Excel y las filas extraídas.
/// </summary>
public record ExcelFieldsResponse(
    List<string> RequestedFields,
    List<string> MissingColumns,
    List<Dictionary<string, string?>> Rows);
