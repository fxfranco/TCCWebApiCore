using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TCC.Api.SystemOneBPMS.Models.DTOs;
using TCC.Api.SystemOneBPMS.Services.Implementations;
using TCC.Api.SystemOneBPMS.Services.Interfaces;

namespace TCC.Api.SystemOneBPMS.Controllers;

/// <summary>
/// Endpoint protegido por JWT que recibe un archivo Excel, lee la lista de
/// campos desde un JSON de configuración y devuelve solo esas columnas.
/// </summary>
[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class ExcelFieldsController : ControllerBase
{
    private readonly IExcelFieldsExtractorService _extractorService;
    private readonly InMemoryExcelStorage _memoryStorage; // El nuevo almacenamiento
    private readonly ILogger<ExcelFieldsController> _logger;

    public ExcelFieldsController(IExcelFieldsExtractorService extractorService, InMemoryExcelStorage memoryStorage, ILogger<ExcelFieldsController> logger)
    {
        _extractorService = extractorService;
        _memoryStorage = memoryStorage;
        _logger = logger;
    }

    /// <summary>
    /// Extrae del Excel adjunto únicamente las columnas listadas en el
    /// archivo JSON de configuración (por defecto "fields-config.json").
    /// Requiere header: Authorization: Bearer {token}
    /// </summary>
    [HttpPost("extract")]
    [Authorize]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ExcelFieldsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Extract([FromForm] ExcelFieldsRequest request, CancellationToken cancellationToken)
    {
        if (request.ExcelFile is null || request.ExcelFile.Length == 0)
        {
            return BadRequest(new { message = "Debe adjuntar un archivo Excel válido (.xlsx)." });
        }

        var configFileName = string.IsNullOrWhiteSpace(request.FieldsConfigFileName)
            ? "fields-config.json"
            : request.FieldsConfigFileName;

        try
        {
            await using var stream = request.ExcelFile.OpenReadStream();
            var result = await _extractorService.ExtractAsync(stream, configFileName, cancellationToken);

            // 2. NUEVO: Guardar la respuesta estructurada en la memoria global de la API
            _memoryStorage.SaveLatestData(result);

            // 3. OPCIONAL: Guardar también el archivo Excel en crudo por si otra App lo requiere
            using var ms = new MemoryStream();
            await request.ExcelFile.CopyToAsync(ms);
            _memoryStorage.SaveLatestFile(request.ExcelFile.FileName, ms.ToArray());

            return Ok(result);
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogWarning(ex, "Archivo de configuración de campos no encontrado.");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException)
        {
            _logger.LogError(ex, "Error al procesar el archivo Excel.");
            return BadRequest(new { message = "No fue posible procesar el archivo Excel. Verifique que sea un .xlsx válido." });
        }
    }

    /// <summary>
    /// ENDPOINT NUEVO: Permite a CUALQUIER otra aplicación o API consumir 
    /// los últimos datos del Excel que están cargados en la memoria del servidor.
    /// </summary>
    [HttpGet("latest-data")]
    public IActionResult GetLatestProcessedData()
    {
        var data = _memoryStorage.GetLatestData();
        if (data == null)
        {
            return NotFound("No hay datos de Excel cargados en la memoria de la API en este momento.");
        }
        return Ok(data);
    }

    /// <summary>
    /// ENDPOINT NUEVO OPCIONAL: Permite a otra aplicación descargar 
    /// el archivo físico de Excel que subió Blazor.
    /// </summary>
    [HttpGet("download-latest")]
    public IActionResult DownloadLatestFile()
    {
        var fileData = _memoryStorage.GetLatestFile();
        if (fileData == null)
        {
            return NotFound("No hay ningún archivo físico en la memoria de la API.");
        }

        return File(
            fileData.Value.Bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileData.Value.Name
        );
    }

    /// <summary>
    /// ENDPOINT NUEVO: Para limpiar la memoria desde Blazor o externamente
    /// </summary>
    [HttpPost("clear-server-memory")]
    public IActionResult ClearServerMemory()
    {
        _memoryStorage.Clear();
        return Ok("Memoria global de la API limpiada correctamente.");
    }
}
