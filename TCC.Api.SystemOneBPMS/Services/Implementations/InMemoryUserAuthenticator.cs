using TCC.Api.SystemOneBPMS.Services.Interfaces;

namespace TCC.Api.SystemOneBPMS.Services.Implementations;

/// <summary>
/// Implementación de demostración con usuarios "hardcodeados".
/// En un escenario real, esta clase se reemplaza por una que consulte
/// una base de datos, Active Directory, o un proveedor de identidad
/// (Azure AD, IdentityServer, etc.) implementando la misma interfaz
/// IUserAuthenticator, sin tocar el resto del sistema (DIP/OCP).
/// </summary>
public class InMemoryUserAuthenticator : IUserAuthenticator
{
    private sealed record UserRecord(string Password, string[] Roles);

    private static readonly Dictionary<string, UserRecord> Users = new(StringComparer.OrdinalIgnoreCase)
    {
        ["admin"] = new UserRecord("Admin123!", new[] { "Admin" }),
        ["user"] = new UserRecord("User123!", new[] { "User" })
    };

    public Task<AuthenticatedUser?> ValidateAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(username)
            && Users.TryGetValue(username, out var record)
            && record.Password == password)
        {
            return Task.FromResult<AuthenticatedUser?>(new AuthenticatedUser(username, record.Roles));
        }

        return Task.FromResult<AuthenticatedUser?>(null);
    }
}
