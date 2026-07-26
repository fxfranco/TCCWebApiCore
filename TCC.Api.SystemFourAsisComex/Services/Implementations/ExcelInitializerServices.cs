using TCC.Api.SystemFourAsisComex.Services.Interfaces;

namespace TCC.Api.SystemFourAsisComex.Services.Implementations
{
    public class ExcelInitializerServices : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHostEnvironment _env;

        public ExcelInitializerServices(IServiceProvider serviceProvider, IHostEnvironment env)
        {
            _serviceProvider = serviceProvider;
            _env = env;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // 1. Ruta física del archivo dentro de la raíz del proyecto
            // Asumiendo que el archivo está en la carpeta 'Data/Plantilla.xlsx'
            String filePath = Directory.GetCurrentDirectory() + "\\Data\\ApplicationDatabase.xlsx";

            if (File.Exists(filePath))
            {

                // 2. Creas un Scope manual para poder usar servicios Scoped de forma segura
                using var scope = _serviceProvider.CreateScope();

                var extractorService = scope.ServiceProvider.GetRequiredService<IExcelFieldsExtractorService>();
                var inMemoryStorage = scope.ServiceProvider.GetRequiredService<InMemoryExcelStorage>();

                // 3. Abrimos el Stream local y reutilizamos tu extractor existente
                await using var stream = File.OpenRead(filePath);

                // Asumiendo que tu método recibe el Stream (si es asíncrono usa await)
                //var result = await extractorService.ExtractFieldsAsync(stream);
                var result = await extractorService.ExtractAsync(stream, "fields-config.json", cancellationToken);

                // 2. NUEVO: Guardar la respuesta estructurada en la memoria global de la API
                inMemoryStorage.SaveLatestData(result);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
