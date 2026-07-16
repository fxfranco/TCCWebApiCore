namespace TCC.Api.SystemTwoSIM.Models.DTOs;

/// <summary>Petición de login.</summary>
public record LoginRequest(string Username, string Password);

/// <summary>Respuesta de login con el token generado.</summary>
public record LoginResponse(string Token, string TokenType, DateTime IssuedAtUtc, DateTime ExpiresAtUtc);
