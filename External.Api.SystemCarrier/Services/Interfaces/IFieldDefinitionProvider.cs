using External.Api.SystemCarrier.Models;

namespace External.Api.SystemCarrier.Services.Interfaces;

/// <summary>
/// Responsable exclusivamente de obtener la lista de campos a extraer (SRP).
/// Hoy lee de un archivo JSON en disco; mañana podría leer de una base de
/// datos o de un endpoint remoto sin que el resto de la app lo note (OCP/DIP).
/// </summary>
public interface IFieldDefinitionProvider
{
    Task<IReadOnlyList<FieldDefinition>> GetFieldDefinitionsAsync(string configFileName, CancellationToken cancellationToken = default);
}
