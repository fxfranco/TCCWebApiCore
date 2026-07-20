using TCC.Web.InternalSystems.Components;
using TCC.Web.InternalSystems.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Configuration.AddJsonFile("Data\\appusers.json", optional: false, reloadOnChange: true);

// Leer la URL desde el archivo de configuración
// 1. Obtener la sección completa de configuraciones como un diccionario
// Cambia la URL por el puerto real de tu API .NET 10 (ver launchSettings.json de la API)
var apiSettings = builder.Configuration.GetSection("ApiSettings").Get<Dictionary<string, string>>();

if (apiSettings != null)
{
    // 2. Recorrer el diccionario y registrar cada HttpClient dinámicamente
    foreach (var api in apiSettings)
    {
        builder.Services.AddHttpClient(api.Key, client =>
        {
            if (!string.IsNullOrEmpty(api.Value))
            {
                client.BaseAddress = new Uri(api.Value);
            }
        });
    }
}

builder.Services.AddScoped<AppStateServiceUser>();
builder.Services.AddScoped<AppStateServiceBPMS>();
builder.Services.AddScoped<AppStateServiceSIM>();
builder.Services.AddScoped<AppStateServiceOpenComex>();
builder.Services.AddScoped<AppStateServiceAsisComex>();
builder.Services.AddScoped<AppStateServiceExternalCarrier>();
builder.Services.AddScoped<AppStateServiceUsersSystem>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
