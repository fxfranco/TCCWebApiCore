using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TCC.Api.SystemFourAsisComex.Models.Configuration;
using TCC.Api.SystemFourAsisComex.Services.Implementations;
using TCC.Api.SystemFourAsisComex.Services.Interfaces;

namespace TCC.Api.SystemFourAsisComex.Extensions;

/// <summary>
/// Centraliza el registro de dependencias (composition root) para mantener
/// Program.cs limpio y respetar el principio DIP: los módulos de alto nivel
/// (controladores) dependen de abstracciones, y aquí se decide qué
/// implementación concreta se usa para cada una.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <param name="dataDirectory">
    /// Carpeta de datos (Excel + JSON). Null = {bin}/Data (API independiente).
    /// La Web integradora envía {bin}/Data/AsisComex.
    /// </param>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, string? dataDirectory = null)
    {
        ApiModule.UseDataDirectory(dataDirectory);

        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IUserAuthenticator, InMemoryUserAuthenticator>();
        services.AddScoped<IFieldDefinitionProvider, JsonFieldDefinitionProvider>();
        services.AddScoped<IExcelReader, ClosedXmlExcelReader>();
        services.AddSingleton<IDateValueNormalizer, ExcelDateValueNormalizer>();
        services.AddScoped<IExcelFieldsExtractorService, ExcelFieldsExtractorService>();

        // Registrar como Singleton para que la memoria sea compartida de forma global
        services.AddSingleton<InMemoryExcelStorage>();

        // Registrar el servicio que lee el Excel al iniciar la API
        services.AddHostedService<ExcelInitializerServices>();

        return services;
    }

    /// <param name="jwtSectionName">
    /// Sección de configuración con los datos JWT. "Jwt" en la API independiente;
    /// "Apis:AsisComex:Jwt" cuando se integra en la Web.
    /// </param>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration, string jwtSectionName = "Jwt")
    {
        var jwtSection = configuration.GetSection(jwtSectionName);
        var jwtSettings = jwtSection.Get<JwtSettings>()
            ?? throw new InvalidOperationException($"La sección '{jwtSectionName}' no está configurada en appsettings.json.");

        if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
        {
            throw new InvalidOperationException($"{jwtSectionName}:SecretKey no puede estar vacío.");
        }

        services.Configure<JwtSettings>(jwtSection);

        // Esquema con nombre propio (ApiModule.AuthScheme): así varias APIs pueden
        // registrarse en el mismo host sin chocar con el esquema "Bearer".
        services.AddAuthentication()
            .AddJwtBearer(ApiModule.AuthScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ClockSkew = TimeSpan.Zero
                };

                // Eventos de diagnóstico: mientras se depura un 401, estos logs
                // muestran la causa exacta (firma inválida, token expirado,
                // issuer/audience incorrectos, header ausente, etc.).
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger($"JwtBearer.{ApiModule.Area}");
                        logger.LogWarning(context.Exception, "Falló la validación del JWT: {Message}", context.Exception.Message);
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger($"JwtBearer.{ApiModule.Area}");
                        logger.LogWarning(
                            "Challenge 401 emitido. Error: {Error}, Descripción: {Description}, Header Authorization presente: {HasAuthHeader}",
                            context.Error,
                            context.ErrorDescription,
                            context.Request.Headers.ContainsKey("Authorization"));
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }
}
