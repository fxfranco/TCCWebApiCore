using External.Api.SystemCarrier.Models.DTOs;
using External.Api.SystemCarrier.Services.Interfaces;

namespace External.Api.SystemCarrier.Services.Implementations;

/// <summary>
/// Orquesta el caso de uso completo: obtiene los campos configurados,
/// lee el Excel y filtra las columnas solicitadas. Depende únicamente
/// de abstracciones (IFieldDefinitionProvider, IExcelReader), nunca de
/// implementaciones concretas (DIP).
/// </summary>
public class ExcelFieldsExtractorService : IExcelFieldsExtractorService
{
    private readonly IFieldDefinitionProvider _fieldDefinitionProvider;
    private readonly IExcelReader _excelReader;

    public ExcelFieldsExtractorService(IFieldDefinitionProvider fieldDefinitionProvider, IExcelReader excelReader)
    {
        _fieldDefinitionProvider = fieldDefinitionProvider;
        _excelReader = excelReader;
    }

    public async Task<ExcelFieldsResponse> ExtractAsync(Stream excelStream, string fieldsConfigFileName, CancellationToken cancellationToken = default)
    {
        var fieldDefinitions = await _fieldDefinitionProvider.GetFieldDefinitionsAsync(fieldsConfigFileName, cancellationToken);
        var rows = await _excelReader.ReadRowsAsync(excelStream, cancellationToken);

        var requestedColumns = fieldDefinitions
            .Select(f => f.Column)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var filteredRows = rows
            .Select(row => row
                .Where(kv => requestedColumns.Contains(kv.Key))
                .ToDictionary(kv => kv.Key, kv => kv.Value))
            .ToList();

        var foundColumns = filteredRows
            .SelectMany(r => r.Keys)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missingColumns = requestedColumns
            .Where(col => !foundColumns.Contains(col))
            .ToList();

        return new ExcelFieldsResponse(
            RequestedFields: fieldDefinitions.Select(f => f.Name).ToList(),
            MissingColumns: missingColumns,
            Rows: filteredRows);
    }
}
