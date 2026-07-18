using System.Text.Json;
using Microsoft.Extensions.Hosting;
using TCC.Api.SystemUsers.Models;
using TCC.Api.SystemUsers.Services.Interfaces;

namespace TCC.Api.SystemUsers.Services.Implementations;

/// <summary>
/// Lee la lista de campos a extraer desde un archivo JSON ubicado en la
/// carpeta Data del proyecto.
/// </summary>
public class JsonFieldDefinitionProvider : IFieldDefinitionProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly string _dataDirectory;

    public JsonFieldDefinitionProvider(IHostEnvironment environment)
    {
        _dataDirectory = Path.Combine(environment.ContentRootPath, "Data");
    }

    public async Task<IReadOnlyList<FieldDefinition>> GetFieldDefinitionsAsync(string configFileName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(configFileName))
        {
            throw new ArgumentException("El nombre del archivo de configuración de campos es obligatorio.", nameof(configFileName));
        }

        // Evita path traversal fuera de la carpeta Data.
        var safeFileName = Path.GetFileName(configFileName);
        var filePath = Path.Combine(_dataDirectory, safeFileName);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                $"No se encontró el archivo de configuración de campos '{safeFileName}'.", filePath);
        }

        await using var stream = File.OpenRead(filePath);
        var config = await JsonSerializer.DeserializeAsync<FieldDefinitionsConfig>(stream, JsonOptions, cancellationToken);

        return config?.Fields ?? new List<FieldDefinition>();
    }
}
