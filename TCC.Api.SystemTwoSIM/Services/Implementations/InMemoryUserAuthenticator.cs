using TCC.Api.SystemTwoSIM.Services.Interfaces;

namespace TCC.Api.SystemTwoSIM.Services.Implementations;

/// <summary>
/// Implementación de demostración con usuarios "hardcodeados".
/// En un escenario real, esta clase se reemplaza por una que consulte
/// una base de datos, Active Directory, o un proveedor de identidad
/// (Azure AD, IdentityServer, etc.) implementando la misma interfaz
/// IUserAuthenticator, sin tocar el resto del sistema (DIP/OCP).
/// </summary>
public class InMemoryUserAuthenticator : IUserAuthenticator
{
    private readonly IConfiguration _configuration;
    public InMemoryUserAuthenticator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private sealed record UserRecord(string Password, string[] Roles);

    public Task<AuthenticatedUser?> ValidateAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var users = _configuration.GetSection("Users").Get<List<UserConfig>>();

        var validUser = users?.FirstOrDefault(u => u.Username == username && u.Password == password);

        if (validUser != null)
        {
            return Task.FromResult<AuthenticatedUser?>(new AuthenticatedUser(username, []));
        }
        else
        {
            return Task.FromResult<AuthenticatedUser?>(null);
        }
    }
}
