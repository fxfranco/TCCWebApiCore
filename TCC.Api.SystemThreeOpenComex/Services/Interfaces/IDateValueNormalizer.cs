namespace TCC.Api.SystemThreeOpenComex.Services.Interfaces;

/// <summary>
/// Normaliza el valor de un campo de tipo fecha leído del Excel al formato ISO
/// "yyyy-MM-dd" (el que espera PostgreSQL para columnas DATE), sin ambigüedad día/mes (SRP).
/// </summary>
public interface IDateValueNormalizer
{
    /// <summary>Formato de salida: año-mes-día.</summary>
    const string OutputFormat = "yyyy-MM-dd";

    /// <summary>
    /// Intenta convertir <paramref name="raw"/> a "yyyy-MM-dd".
    /// Devuelve false si el valor no se reconoce como fecha (el llamador conserva el valor original).
    /// </summary>
    bool TryNormalize(string? raw, out string normalized);
}
