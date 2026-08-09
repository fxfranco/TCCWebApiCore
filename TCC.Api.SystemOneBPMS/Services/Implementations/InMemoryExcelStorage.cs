using System.Collections.Concurrent;

namespace TCC.Api.SystemOneBPMS.Services.Implementations
{
    public class InMemoryExcelStorage
    {
        // Usamos ConcurrentDictionary por seguridad de hilos (Thread-safe) ya que múltiples APIs/Apps pueden llamarlo a la vez
        private readonly ConcurrentDictionary<string, byte[]> _rawFiles = new();
        private readonly ConcurrentDictionary<string, object> _processedData = new();
        private readonly ConcurrentDictionary<string, object> _processedDataLogs = new();

        // Almacenar y obtener el último Excel procesado de forma genérica
        public void SaveLatestData(object response)
        {
            _processedData["latest"] = response;
        }

        // Almacenar y obtener el último Excel procesado de forma genérica
        public void SaveLatestLogsData(object response)
        {
            _processedDataLogs["latest"] = response;
        }

        public object? GetLatestData()
        {
            _processedData.TryGetValue("latest", out var data);
            return data;
        }

        public object? GetLatestLogsData()
        {
            _processedDataLogs.TryGetValue("latest", out var data);
            return data;
        }

        // OPCIONAL: Por si otra app necesita el archivo .xlsx real en bytes
        public void SaveLatestFile(string fileName, byte[] fileBytes)
        {
            _rawFiles["latest_name"] = System.Text.Encoding.UTF8.GetBytes(fileName);
            _rawFiles["latest_bytes"] = fileBytes;
        }

        public (string Name, byte[] Bytes)? GetLatestFile()
        {
            if (_rawFiles.TryGetValue("latest_bytes", out var bytes) &&
                _rawFiles.TryGetValue("latest_name", out var nameBytes))
            {
                return (System.Text.Encoding.UTF8.GetString(nameBytes), bytes);
            }
            return null;
        }

        // Limpiar la memoria del servidor si se solicita
        public void Clear()
        {
            _rawFiles.Clear();
            _processedData.Clear();
            _processedDataLogs.Clear();
        }
    }
}
