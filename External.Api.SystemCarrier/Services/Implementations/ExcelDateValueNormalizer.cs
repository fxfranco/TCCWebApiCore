using System.Globalization;
using System.Text.RegularExpressions;
using External.Api.SystemCarrier.Services.Interfaces;

namespace External.Api.SystemCarrier.Services.Implementations;

/// <summary>
/// Convierte a "yyyy-MM-dd" las fechas que llegan desde el Excel, en cualquiera de estas formas:
///   - ISO:                       2026-02-05 / 2026-02-05T00:00:00 / 2026-02-05 00:00:00
///   - Día + mes abreviado:       14/jul/2025, 2-may-26, 02/sept./2025, 5 ene 2026  (español o inglés)
///   - Día/mes numérico:          14/07/2025, 2-5-26  (siempre DÍA primero, convención de Colombia)
///   - Texto heredado invariante: 08/03/2025 00:00:00 (MM/dd/yyyy HH:mm:ss, como lo generaba la versión anterior)
///   - Número serial de Excel:    46173 (celdas con formato General)
/// No depende de la cultura del sistema (las APIs usan InvariantGlobalization).
/// </summary>
public sealed class ExcelDateValueNormalizer : IDateValueNormalizer
{
    private static readonly Dictionary<string, int> Months = new(StringComparer.OrdinalIgnoreCase)
    {
        // Español
        ["ene"] = 1, ["enero"] = 1,
        ["feb"] = 2, ["febrero"] = 2,
        ["mar"] = 3, ["marzo"] = 3,
        ["abr"] = 4, ["abril"] = 4,
        ["may"] = 5, ["mayo"] = 5,
        ["jun"] = 6, ["junio"] = 6,
        ["jul"] = 7, ["julio"] = 7,
        ["ago"] = 8, ["agosto"] = 8,
        ["sep"] = 9, ["sept"] = 9, ["set"] = 9, ["septiembre"] = 9, ["setiembre"] = 9,
        ["oct"] = 10, ["octubre"] = 10,
        ["nov"] = 11, ["noviembre"] = 11,
        ["dic"] = 12, ["diciembre"] = 12,
        // Inglés (solo los que difieren)
        ["jan"] = 1, ["january"] = 1, ["february"] = 2, ["march"] = 3,
        ["apr"] = 4, ["april"] = 4, ["june"] = 6, ["july"] = 7,
        ["aug"] = 8, ["august"] = 8, ["september"] = 9, ["october"] = 10,
        ["november"] = 11, ["dec"] = 12, ["december"] = 12
    };

    // 2026-02-05 | 2026-02-05T10:30:00 | 2026-02-05 00:00:00
    private static readonly Regex IsoRegex =
        new(@"^(?<y>\d{4})-(?<m>\d{1,2})-(?<d>\d{1,2})(?:[T ].*)?$", RegexOptions.Compiled);

    // 14/jul/2025 | 2-may-26 | 02/sept./2025 | 5 ene 2026
    private static readonly Regex DayMonthNameYearRegex =
        new(@"^(?<d>\d{1,2})\s*[/\-. ]\s*(?<mon>[A-Za-zÁÉÍÓÚáéíóú]+)\.?\s*[/\-. ]\s*(?<y>\d{2}|\d{4})$", RegexOptions.Compiled);

    // 14/07/2025 | 2-5-26 | 08/03/2025 00:00:00 (con hora)
    private static readonly Regex NumericRegex =
        new(@"^(?<a>\d{1,2})[/\-.](?<b>\d{1,2})[/\-.](?<y>\d{2}|\d{4})(?<time>\s+\d{1,2}:\d{2}(?::\d{2})?(?:\s*[AaPp]\.?\s*[Mm]\.?)?)?$", RegexOptions.Compiled);

    public bool TryNormalize(string? raw, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        var value = raw.Trim();

        // 1) ISO
        var iso = IsoRegex.Match(value);
        if (iso.Success)
        {
            return TryBuild(iso.Groups["y"].Value, iso.Groups["m"].Value, iso.Groups["d"].Value, out normalized);
        }

        // 2) Día + nombre/abreviatura del mes + año (formato de visualización del Excel)
        var named = DayMonthNameYearRegex.Match(value);
        if (named.Success)
        {
            var monthText = named.Groups["mon"].Value.TrimEnd('.');
            return Months.TryGetValue(monthText, out var month)
                   && TryBuild(named.Groups["y"].Value, month.ToString(CultureInfo.InvariantCulture), named.Groups["d"].Value, out normalized);
        }

        // 3) Numérico d/m/a. Con hora => texto heredado invariante (MM/dd/yyyy HH:mm:ss): mes primero.
        //    Sin hora => día primero (dd/MM/yyyy).
        var numeric = NumericRegex.Match(value);
        if (numeric.Success)
        {
            var hasTime = numeric.Groups["time"].Success;
            var day = hasTime ? numeric.Groups["b"].Value : numeric.Groups["a"].Value;
            var month = hasTime ? numeric.Groups["a"].Value : numeric.Groups["b"].Value;
            return TryBuild(numeric.Groups["y"].Value, month, day, out normalized);
        }

        // 4) Número serial de Excel (días desde 1899-12-30). Rango razonable: 1950..2100
        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var serial)
            && serial >= 18264 && serial <= 73050)
        {
            normalized = DateTime.FromOADate(serial).ToString(IDateValueNormalizer.OutputFormat, CultureInfo.InvariantCulture);
            return true;
        }

        return false;
    }

    private static bool TryBuild(string yearText, string monthText, string dayText, out string normalized)
    {
        normalized = string.Empty;
        if (!int.TryParse(yearText, NumberStyles.None, CultureInfo.InvariantCulture, out var year)
            || !int.TryParse(monthText, NumberStyles.None, CultureInfo.InvariantCulture, out var month)
            || !int.TryParse(dayText, NumberStyles.None, CultureInfo.InvariantCulture, out var day))
        {
            return false;
        }

        if (yearText.Length == 2)
        {
            year += 2000; // 26 -> 2026
        }

        if (month is < 1 or > 12 || day < 1 || year < 1 || day > DateTime.DaysInMonth(year, month))
        {
            return false;
        }

        normalized = new DateTime(year, month, day).ToString(IDateValueNormalizer.OutputFormat, CultureInfo.InvariantCulture);
        return true;
    }
}
