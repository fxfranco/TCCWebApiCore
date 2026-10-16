using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace TCC.Api.SystemThreeOpenComex.Services.Implementations
{
    public static class ApplicationDatabaseFileManager
    {
        private const string NombreBase = "ApplicationDatabase";
        private const string Extension = ".xlsx";

        /// <summary>
        /// Obtiene la ruta del último archivo existente (para LEER datos).
        /// </summary>
        public static string? ObtenerUltimoArchivoExistente()
        {
            var carpetaData = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(carpetaData)) return null;

            var archivosEncontrados = Directory.EnumerateFiles(carpetaData, "*.xlsx")
                .Select(ruta =>
                {
                    var nombreConExtension = Path.GetFileName(ruta);
                    var nombreSinExtension = Path.GetFileNameWithoutExtension(ruta);

                    // 1. Validar que la extensión coincida exactamente (.xlsx)
                    if (!nombreConExtension.EndsWith(Extension, StringComparison.OrdinalIgnoreCase))
                    {
                        return new { Ruta = ruta, EsValido = false, Indice = -1 };
                    }

                    // 2. CASO EXACTO: "ApplicationDatabase.xlsx" (sin sufijo) -> Índice 0
                    if (string.Equals(nombreSinExtension, NombreBase, StringComparison.OrdinalIgnoreCase))
                    {
                        return new { Ruta = ruta, EsValido = true, Indice = 0 };
                    }

                    // 3. CASO CON SUFIJO: "ApplicationDatabase_1.xlsx", "ApplicationDatabase_2.xlsx", etc.
                    string prefijoEsperado = NombreBase + "_";
                    if (nombreSinExtension.StartsWith(prefijoEsperado, StringComparison.OrdinalIgnoreCase))
                    {
                        string parteNumerica = nombreSinExtension.Substring(prefijoEsperado.Length);
                        if (int.TryParse(parteNumerica, out int indice))
                        {
                            return new { Ruta = ruta, EsValido = true, Indice = indice };
                        }
                    }

                    return new { Ruta = ruta, EsValido = false, Indice = -1 };
                })
                .Where(x => x.EsValido)
                .OrderByDescending(x => x.Indice)
                .ToList();

            return archivosEncontrados.FirstOrDefault()?.Ruta;
        }

        /// <summary>
        /// Obtiene la ruta donde se debe GUARDAR el nuevo archivo.
        /// </summary>
        public static string ObtenerRutaSiguienteArchivo()
        {
            var carpetaData = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(carpetaData))
            {
                Directory.CreateDirectory(carpetaData);
            }

            var ultimoArchivo = ObtenerUltimoArchivoExistente();

            // Si no hay ningún archivo en la carpeta, crear el base sin sufijo
            if (ultimoArchivo == null)
            {
                return Path.Combine(carpetaData, $"{NombreBase}{Extension}");
            }

            // Si ya existe al menos uno, determinar si el último era con o sin sufijo
            var nombreSinExtension = Path.GetFileNameWithoutExtension(ultimoArchivo);

            if (string.Equals(nombreSinExtension, NombreBase, StringComparison.OrdinalIgnoreCase))
            {
                // Era "ApplicationDatabase.xlsx" -> El siguiente es "ApplicationDatabase_1.xlsx"
                return Path.Combine(carpetaData, $"{NombreBase}_1{Extension}");
            }

            string prefijoEsperado = NombreBase + "_";
            if (nombreSinExtension.StartsWith(prefijoEsperado, StringComparison.OrdinalIgnoreCase))
            {
                string parteNumerica = nombreSinExtension.Substring(prefijoEsperado.Length);
                if (int.TryParse(parteNumerica, out int ultimoIndice))
                {
                    return Path.Combine(carpetaData, $"{NombreBase}_{ultimoIndice + 1}{Extension}");
                }
            }

            // Fallback por seguridad
            return Path.Combine(carpetaData, $"{NombreBase}_1{Extension}");
        }
    }
}
