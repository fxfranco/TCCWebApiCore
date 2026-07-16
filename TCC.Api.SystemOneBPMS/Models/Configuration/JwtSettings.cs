namespace TCC.Api.SystemOneBPMS.Models.Configuration;

/// <summary>
/// Opciones de configuración para la generación y validación de JWT.
/// Se enlaza a la sección "Jwt" de appsettings.json (patrón Options).
/// </summary>
public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
}
