using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TCC.Api.SystemThreeOpenComex.Models.DTOs;
using TCC.Api.SystemThreeOpenComex.Services.Implementations;
using TCC.Api.SystemThreeOpenComex.Services.Interfaces;

namespace TCC.Api.SystemThreeOpenComex.Controllers;

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

    [HttpPost("save-in-memory")]
    [Authorize]
    public IActionResult SaveInMemoryAndExport([FromBody] GuardarDatosRequest request)
    {
        if (request == null || request.Registros == null || !request.Registros.Any())
        {
            return BadRequest("No se proporcionaron registros válidos.");
        }

        if (string.IsNullOrWhiteSpace(request.RutaArchivo))
        {
            return BadRequest("La ruta del archivo de salida es obligatoria.");
        }

        try
        {
            // 1. Guardar la información en la memoria de la API
            //_memoryService.DatosAlmacenados = request.Registros;
            _memoryStorage.SaveLatestData(request.Registros);

            // 2. Generar el archivo Excel Dinámico usando EPPlus
            // 2. Generar el archivo Excel Dinámico usando ClosedXML
            using (var workbook = new XLWorkbook())
            {
                // Crear la hoja de trabajo
                var worksheet = workbook.Worksheets.Add("Datos BPMS");

                // Extraemos todas las columnas dinámicas de los diccionarios enviados
                var columnas = request.Registros
                    .SelectMany(d => d.Keys)
                    .Distinct()
                    .ToList();

                // Pintar Cabeceras (Fila 1) - ClosedXML usa base 1 para filas y columnas
                for (int colIndex = 0; colIndex < columnas.Count; colIndex++)
                {
                    var celdaCabecera = worksheet.Cell(1, colIndex + 1);
                    celdaCabecera.Value = columnas[colIndex];
                    celdaCabecera.Style.Font.Bold = true;
                }

                // Pintar Filas de Datos (Desde fila 2)
                int rowIndex = 2;
                foreach (var fila in request.Registros)
                {
                    for (int colIndex = 0; colIndex < columnas.Count; colIndex++)
                    {
                        string nombreColumna = columnas[colIndex];
                        string? valorCelda = fila.ContainsKey(nombreColumna) ? fila[nombreColumna] : string.Empty;

                        // ClosedXML maneja de manera segura los strings asignándolos al valor de la celda
                        worksheet.Cell(rowIndex, colIndex + 1).Value = valorCelda ?? string.Empty;
                    }
                    rowIndex++;
                }

                // Autoajustar las columnas al ancho del contenido de forma dinámica
                worksheet.Columns().AdjustToContents();

                // Asegurar que el directorio destino de la ruta exista físicamente
                var directorio = Path.GetDirectoryName(request.RutaArchivo);
                if (!string.IsNullOrEmpty(directorio) && !Directory.Exists(directorio))
                {
                    Directory.CreateDirectory(directorio);
                }

                // Guardar el libro de trabajo de ClosedXML en la ruta dada
                workbook.SaveAs(request.RutaArchivo);
            }

            return Ok(new { mensaje = "Datos procesados y archivo guardado de forma exitosa." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno al generar el archivo en el servidor: {ex.Message}");
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
