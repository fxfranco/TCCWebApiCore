using Microsoft.AspNetCore.Mvc;
using TCC.Api.SystemThreeOpenComex.Models.DTOs;
using TCC.Api.SystemThreeOpenComex.Services.Interfaces;

namespace TCC.Api.SystemThreeOpenComex.Controllers;

/// <summary>
/// Expone el endpoint de login. Solo coordina IUserAuthenticator + ITokenService;
/// no conoce detalles de cómo se validan credenciales ni de cómo se firma el JWT (SRP/DIP).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserAuthenticator _authenticator;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserAuthenticator authenticator, ITokenService tokenService, ILogger<AuthController> logger)
    {
        _authenticator = authenticator;
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>
    /// Valida credenciales y, si son correctas, devuelve un JWT.
    /// Usuarios de demo: admin/Admin123!  |  user/User123!
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Usuario y contraseña son obligatorios." });
        }

        var user = await _authenticator.ValidateAsync(request.Username, request.Password, cancellationToken);

        if (user is null)
        {

            _logger.LogWarning("Intento de login fallido para el usuario {Username}", request.Username);
            return Unauthorized(new { message = "Credenciales inválidas." });
        }

        var (token, issuedAt, expiresAt) = _tokenService.GenerateToken(user.UserName, user.Roles);

        return Ok(new LoginResponse(token, "Bearer", issuedAt, expiresAt));
    }
}
