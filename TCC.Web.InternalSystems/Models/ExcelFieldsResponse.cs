namespace TCC.Web.InternalSystems.Models;

/// <summary>
/// Estructura completa que devuelven los endpoints latest-data / extract de cada API
/// y que la API guarda en su memoria. La Web la conserva tal cual (en el AppState de
/// cada sistema) y la devuelve completa al guardar, para que la memoria de la API
/// mantenga siempre el mismo formato JSON.
/// </summary>
public class ExcelFieldsResponse
{
    public List<string> RequestedFields { get; set; } = new();
    public List<string> MissingColumns { get; set; } = new();
    public List<Dictionary<string, string?>> Rows { get; set; } = new();
}
