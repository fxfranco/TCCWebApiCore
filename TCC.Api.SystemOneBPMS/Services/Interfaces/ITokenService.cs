namespace TCC.Api.SystemOneBPMS.Services.Interfaces;

/// <summary>
/// Responsable exclusivamente de generar tokens JWT (SRP).
/// Cualquier estrategia de firmado/formato de token puede implementarse
/// sin afectar a los consumidores (OCP/DIP).
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Genera un JWT firmado para el usuario y roles indicados.
    /// </summary>
    (string Token, DateTime IssuedAtUtc, DateTime ExpiresAtUtc) GenerateToken(string userName, IEnumerable<string> roles);
}
