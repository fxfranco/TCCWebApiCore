using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TCC.Api.SystemThreeOpenComex.Models.Configuration;
using TCC.Api.SystemThreeOpenComex.Services.Implementations;
using TCC.Api.SystemThreeOpenComex.Services.Interfaces;

namespace TCC.Api.SystemThreeOpenComex.Extensions;

/// <summary>
/// Centraliza el registro de dependencias (composition root) para mantener
/// Program.cs limpio y respetar el principio DIP: los módulos de alto nivel
/// (controladores) dependen de abstracciones, y aquí se decide qué
/// implementación concreta se usa para cada una.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IUserAuthenticator, InMemoryUserAuthenticator>();
        services.AddScoped<IFieldDefinitionProvider, JsonFieldDefinitionProvider>();
        services.AddScoped<IExcelReader, ClosedXmlExcelReader>();
        services.AddScoped<IExcelFieldsExtractorService, ExcelFieldsExtractorService>();

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>()
            ?? throw new InvalidOperationException("La sección 'Jwt' no está configurada en appsettings.json.");

        if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
        {
            throw new InvalidOperationException("Jwt:SecretKey no puede estar vacío.");
        }

        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
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
                // Se pueden quitar una vez confirmado que todo funciona.
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("JwtBearer");
                        logger.LogWarning(context.Exception, "Falló la validación del JWT: {Message}", context.Exception.Message);
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("JwtBearer");
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
