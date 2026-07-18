using External.Api.SystemCarrier.Models.DTOs;

namespace External.Api.SystemCarrier.Services.Interfaces;

/// <summary>
/// Orquesta el caso de uso: obtener definición de campos + leer Excel +
/// filtrar/mapear resultado. No sabe CÓMO se leen los campos ni CÓMO se lee
/// el Excel; solo coordina las abstracciones inyectadas (SRP + DIP).
/// </summary>
public interface IExcelFieldsExtractorService
{
    Task<ExcelFieldsResponse> ExtractAsync(Stream excelStream, string fieldsConfigFileName, CancellationToken cancellationToken = default);
}
