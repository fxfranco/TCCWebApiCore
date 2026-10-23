namespace TCC.Web.InternalSystems.Models;

/// <summary>
/// Campos técnicos que viajan en cada registro entre la Web y las APIs pero que
/// no se muestran en la tabla ni en el modal de agregar/modificar.
/// </summary>
public static class CamposSistema
{
    /// <summary>
    /// Usuario que hizo el último cambio (agregar/modificar) sobre el registro.
    /// Debe coincidir con ApiModule.ChangeUserField de las APIs.
    /// </summary>
    public const string UsuarioCambio = "changeuser";

    private static readonly HashSet<string> Ocultos = new(StringComparer.OrdinalIgnoreCase) { UsuarioCambio };

    public static bool EsOculto(string? columna) => columna is not null && Ocultos.Contains(columna);
}
