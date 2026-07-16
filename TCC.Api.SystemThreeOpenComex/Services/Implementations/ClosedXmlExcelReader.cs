using ClosedXML.Excel;
using TCC.Api.SystemThreeOpenComex.Services.Interfaces;

namespace TCC.Api.SystemThreeOpenComex.Services.Implementations;

/// <summary>
/// Implementación de IExcelReader basada en ClosedXML. Es la única clase
/// del proyecto que conoce la librería concreta de lectura de Excel;
/// si mañana se cambia por otra librería, solo esta clase se modifica (SRP/OCP).
/// </summary>
public class ClosedXmlExcelReader : IExcelReader
{
    public Task<IReadOnlyList<IReadOnlyDictionary<string, string?>>> ReadRowsAsync(Stream excelStream, CancellationToken cancellationToken = default)
    {
        using var workbook = new XLWorkbook(excelStream);
        var worksheet = workbook.Worksheets.FirstOrDefault()
            ?? throw new InvalidOperationException("El archivo Excel no contiene ninguna hoja.");

        var headerRow = worksheet.Row(1);
        var lastColumn = headerRow.LastCellUsed()?.Address.ColumnNumber ?? 0;

        var headers = new List<string>();
        for (var col = 1; col <= lastColumn; col++)
        {
            headers.Add(headerRow.Cell(col).GetString().Trim());
        }

        var result = new List<IReadOnlyDictionary<string, string?>>();
        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;

        for (var rowNumber = 2; rowNumber <= lastRow; rowNumber++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var row = worksheet.Row(rowNumber);
            var rowData = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            for (var col = 0; col < headers.Count; col++)
            {
                if (string.IsNullOrWhiteSpace(headers[col]))
                {
                    continue;
                }

                var cell = row.Cell(col + 1);
                rowData[headers[col]] = cell.IsEmpty() ? null : cell.GetString();
            }

            result.Add(rowData);
        }

        return Task.FromResult<IReadOnlyList<IReadOnlyDictionary<string, string?>>>(result);
    }
}
