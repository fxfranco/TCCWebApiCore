namespace TCC.Web.InternalSystems.Models;

/// <summary>
/// Catálogo único de estados permitidos para la columna "Estado" de los sistemas.
/// En el modal de agregar/modificar la columna se muestra como lista desplegable con
/// estas opciones y se guarda el TEXTO del estado.
/// </summary>
public static class EstadosRegistro
{
    /// <summary>Nombre de la columna (se compara sin distinguir mayúsculas: "ESTADO", "Estado").</summary>
    public const string NombreColumna = "Estado";

    public static IReadOnlyList<string> Opciones { get; } = new[]
    {
        "Pendiente",
        "En Aduana origen",
        "En tránsito",
        "En Aduana destino",
        "Entregado",
        "Con novedad"
    };

    public static bool EsColumnaEstado(string? columna) =>
        string.Equals(columna?.Trim(), NombreColumna, StringComparison.OrdinalIgnoreCase);

    public static bool EsValido(string? valor) =>
        valor is not null && Opciones.Contains(valor);

    /// <summary>
    /// Devuelve la opción oficial que corresponde al valor (ignorando mayúsculas y espacios),
    /// por ejemplo "en TRÁNSITO " -> "En tránsito". Si no corresponde a ninguna, devuelve el valor tal cual.
    /// </summary>
    public static string? Normalizar(string? valor) =>
        Opciones.FirstOrDefault(o => string.Equals(o, valor?.Trim(), StringComparison.OrdinalIgnoreCase)) ?? valor;
}
