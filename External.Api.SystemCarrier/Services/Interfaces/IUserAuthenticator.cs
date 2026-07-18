namespace External.Api.SystemCarrier.Services.Interfaces;

/// <summary>Usuario autenticado con sus roles.</summary>
public record AuthenticatedUser(string UserName, IReadOnlyList<string> Roles);

/// <summary>
/// Responsable exclusivamente de validar credenciales (SRP).
/// La implementación puede cambiar de "en memoria" a base de datos o un
/// proveedor de identidad externo sin tocar el resto de la aplicación (DIP).
/// </summary>
public interface IUserAuthenticator
{
    Task<AuthenticatedUser?> ValidateAsync(string username, string password, CancellationToken cancellationToken = default);
}
