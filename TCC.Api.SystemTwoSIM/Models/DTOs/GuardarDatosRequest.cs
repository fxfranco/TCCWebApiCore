namespace TCC.Api.SystemTwoSIM.Models.DTOs
{
    public class GuardarDatosRequest
    {
        public string RutaArchivo { get; set; } = string.Empty;
        public List<Dictionary<string, string?>> Registros { get; set; } = new();
    }
}
