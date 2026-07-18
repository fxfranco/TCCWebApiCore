namespace External.Api.SystemCarrier.Services.Interfaces;

/// <summary>
/// Responsable exclusivamente de leer un archivo Excel y devolver sus filas
/// como diccionarios columna-valor (SRP). La librería concreta usada
/// (ClosedXML, EPPlus, etc.) queda oculta detrás de esta abstracción,
/// permitiendo sustituirla sin impactar al resto del sistema (OCP/DIP).
/// </summary>
public interface IExcelReader
{
    /// <summary>
    /// Lee la primera hoja del libro, toma la primera fila como encabezado
    /// y devuelve el resto de filas como diccionarios columna-valor.
    /// </summary>
    Task<IReadOnlyList<IReadOnlyDictionary<string, string?>>> ReadRowsAsync(Stream excelStream, CancellationToken cancellationToken = default);
}
