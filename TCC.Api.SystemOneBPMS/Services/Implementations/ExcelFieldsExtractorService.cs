using TCC.Api.SystemOneBPMS.Models.DTOs;
using TCC.Api.SystemOneBPMS.Services.Interfaces;

namespace TCC.Api.SystemOneBPMS.Services.Implementations;

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
    private readonly IDateValueNormalizer _dateNormalizer;

    public ExcelFieldsExtractorService(
        IFieldDefinitionProvider fieldDefinitionProvider,
        IExcelReader excelReader,
        IDateValueNormalizer dateNormalizer)
    {
        _fieldDefinitionProvider = fieldDefinitionProvider;
        _excelReader = excelReader;
        _dateNormalizer = dateNormalizer;
    }

    public async Task<ExcelFieldsResponse> ExtractAsync(Stream excelStream, string fieldsConfigFileName, CancellationToken cancellationToken = default, bool includeChangeUser = true)
    {
        var fieldDefinitions = await _fieldDefinitionProvider.GetFieldDefinitionsAsync(fieldsConfigFileName, cancellationToken);
        var rows = await _excelReader.ReadRowsAsync(excelStream, cancellationToken);

        var requestedColumns = fieldDefinitions
            .Select(f => f.Column)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Columnas configuradas como "type": "date" en el JSON de campos
        var dateColumns = fieldDefinitions
            .Where(f => f.IsDate)
            .Select(f => f.Column)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var filteredRows = rows
            .Select(row => row
                .Where(kv => requestedColumns.Contains(kv.Key))
                .ToDictionary(kv => kv.Key, kv => NormalizeValue(kv.Key, kv.Value, dateColumns)))
            .ToList();

        var requestedFields = fieldDefinitions.Select(f => f.Name).ToList();

        // Campo "changeuser": siempre presente en cada registro como si viniera en el Excel.
        // Si el archivo ya lo trae (ApplicationDatabase_N.xlsx guardados) se conserva su valor;
        // si no (Excel original), queda vacío. No se reporta en MissingColumns.
        if (includeChangeUser)
        {
            for (var i = 0; i < filteredRows.Count; i++)
            {
                filteredRows[i][ApiModule.ChangeUserField] =
                    rows[i].TryGetValue(ApiModule.ChangeUserField, out var usuario) ? usuario ?? string.Empty : string.Empty;
            }

            if (!requestedFields.Contains(ApiModule.ChangeUserField, StringComparer.OrdinalIgnoreCase))
            {
                requestedFields.Add(ApiModule.ChangeUserField);
            }
        }

        var foundColumns = filteredRows
            .SelectMany(r => r.Keys)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missingColumns = requestedColumns
            .Where(col => !foundColumns.Contains(col))
            .ToList();

        return new ExcelFieldsResponse(
            RequestedFields: requestedFields,
            MissingColumns: missingColumns,
            Rows: filteredRows);
    }

    /// <summary>
    /// Campos de fecha => "yyyy-MM-dd". Si el valor no se reconoce como fecha se conserva tal cual.
    /// </summary>
    private string? NormalizeValue(string column, string? value, HashSet<string> dateColumns)
    {
        if (value is null || !dateColumns.Contains(column))
        {
            return value;
        }

        return _dateNormalizer.TryNormalize(value, out var normalized) ? normalized : value;
    }
}
