namespace TCC.Api.SystemOneBPMS.Models.DTOs
{
    public class GuardarDatosRequest
    {
        public string RutaArchivo { get; set; } = string.Empty;

        /// <summary>
        /// Estructura completa (la misma que devuelve latest-data). Es la forma recomendada.
        /// </summary>
        public ExcelFieldsResponse? Datos { get; set; }

        /// <summary>
        /// Compatibilidad: solo las filas. Se usa si no se envía Datos.
        /// </summary>
        public List<Dictionary<string, string?>> Registros { get; set; } = new();
    }
}
