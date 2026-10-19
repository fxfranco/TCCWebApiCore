using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;
using TCC.Web.InternalSystems.Components;
using TCC.Web.InternalSystems.Infrastructure;
using TCC.Web.InternalSystems.Services;
using CarrierModule = External.Api.SystemCarrier.ApiModule;
using SystemCarrier = External.Api.SystemCarrier.Extensions.ServiceCollectionExtensions;
using AsisComexModule = TCC.Api.SystemFourAsisComex.ApiModule;
using SystemFourAsisComex = TCC.Api.SystemFourAsisComex.Extensions.ServiceCollectionExtensions;
using BpmsModule = TCC.Api.SystemOneBPMS.ApiModule;
using SystemOneBPMS = TCC.Api.SystemOneBPMS.Extensions.ServiceCollectionExtensions;
using OpenComexModule = TCC.Api.SystemThreeOpenComex.ApiModule;
using SystemThreeOpenComex = TCC.Api.SystemThreeOpenComex.Extensions.ServiceCollectionExtensions;
using SimModule = TCC.Api.SystemTwoSIM.ApiModule;
using SystemTwoSIM = TCC.Api.SystemTwoSIM.Extensions.ServiceCollectionExtensions;
using UsersModule = TCC.Api.SystemUsers.ApiModule;
using SystemUsers = TCC.Api.SystemUsers.Extensions.ServiceCollectionExtensions;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Carpeta de datos (Excel + JSON). NO va dentro de la imagen Docker: se monta en el servidor.
//   DataRoot vacío  -> {bin}/Data  (Visual Studio / dotnet run, los archivos se copian al compilar)
//   DataRoot=/app/Data (Docker)  o cualquier ruta absoluta / relativa al directorio de la app.
// Estructura esperada:
//   {DataRoot}/appusers.json
//   {DataRoot}/{BPMS|SIM|OpenComex|AsisComex|Carrier|Users}/ApplicationDatabase.xlsx, fields-config.json, ...
// ---------------------------------------------------------------------------
var configuredDataRoot = builder.Configuration["DataRoot"];
var dataRoot = string.IsNullOrWhiteSpace(configuredDataRoot)
    ? Path.Combine(AppContext.BaseDirectory, "Data")
    : Path.GetFullPath(configuredDataRoot, AppContext.BaseDirectory);

var usersFile = Path.Combine(dataRoot, "appusers.json");
if (!File.Exists(usersFile))
{
    throw new FileNotFoundException(
        $"No se encontró '{usersFile}'. La carpeta Data no viene dentro de la imagen: monte la carpeta de datos " +
        $"en '{dataRoot}' (docker run -v <ruta-host>/Data:/app/Data ...) o configure 'DataRoot'.", usersFile);
}

// Usuarios (login de la Web y de cada API integrada)
builder.Configuration.AddJsonFile(usersFile, optional: false, reloadOnChange: true);

// Registrar los servicios de salud
builder.Services.AddHealthChecks();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ---------------------------------------------------------------------------
// APIs integradas: cada una registra sus servicios, su carga de Excel al iniciar
// (IHostedService) y su propio esquema JWT. Sus datos viven en {DataRoot}/{Area}.
// La configuración JWT de cada una está en appsettings.json -> Apis:{Area}:Jwt
// ---------------------------------------------------------------------------

SystemOneBPMS.AddApplicationServices(builder.Services, Path.Combine(dataRoot, BpmsModule.Area));
SystemOneBPMS.AddJwtAuthentication(builder.Services, builder.Configuration, $"Apis:{BpmsModule.Area}:Jwt");

SystemTwoSIM.AddApplicationServices(builder.Services, Path.Combine(dataRoot, SimModule.Area));
SystemTwoSIM.AddJwtAuthentication(builder.Services, builder.Configuration, $"Apis:{SimModule.Area}:Jwt");

SystemThreeOpenComex.AddApplicationServices(builder.Services, Path.Combine(dataRoot, OpenComexModule.Area));
SystemThreeOpenComex.AddJwtAuthentication(builder.Services, builder.Configuration, $"Apis:{OpenComexModule.Area}:Jwt");

SystemFourAsisComex.AddApplicationServices(builder.Services, Path.Combine(dataRoot, AsisComexModule.Area));
SystemFourAsisComex.AddJwtAuthentication(builder.Services, builder.Configuration, $"Apis:{AsisComexModule.Area}:Jwt");

SystemCarrier.AddApplicationServices(builder.Services, Path.Combine(dataRoot, CarrierModule.Area));
SystemCarrier.AddJwtAuthentication(builder.Services, builder.Configuration, $"Apis:{CarrierModule.Area}:Jwt");

SystemUsers.AddApplicationServices(builder.Services, Path.Combine(dataRoot, UsersModule.Area));
SystemUsers.AddJwtAuthentication(builder.Services, builder.Configuration, $"Apis:{UsersModule.Area}:Jwt");

// Controllers de las APIs (se descubren automáticamente desde los proyectos referenciados)
builder.Services.AddControllers();

// Swagger: un documento por API (área) en /swagger
var apiAreas = new[] { BpmsModule.Area, SimModule.Area, OpenComexModule.Area, AsisComexModule.Area, CarrierModule.Area, UsersModule.Area };
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    foreach (var area in apiAreas)
    {
        options.SwaggerDoc(area, new OpenApiInfo { Title = $"API {area}", Version = "v1" });
    }

    // Cada endpoint va al documento de su área
    options.DocInclusionPredicate((docName, apiDesc) =>
        apiDesc.ActionDescriptor.RouteValues.TryGetValue("area", out var area)
        && string.Equals(area, docName, StringComparison.OrdinalIgnoreCase));

    // Las 6 APIs tienen DTOs con el mismo nombre (LoginRequest, ...): usar el nombre completo
    options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pegue ÚNICAMENTE el token (sin la palabra 'Bearer'); Swagger la agrega automáticamente."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

// ---------------------------------------------------------------------------
// HttpClients de las páginas: por defecto apuntan a esta misma aplicación
// (las APIs ahora viven aquí). Si en ApiSettings se pone una URL absoluta,
// se usa esa URL (por ejemplo para apuntar a una API externa).
// ---------------------------------------------------------------------------
builder.Services.AddSingleton<InternalApiBaseAddress>();

var apiSettings = builder.Configuration.GetSection("ApiSettings").Get<Dictionary<string, string>>()
                  ?? new Dictionary<string, string>();

foreach (var api in apiSettings)
{
    var configuredUrl = api.Value;
    builder.Services.AddHttpClient(api.Key, (sp, client) =>
    {
        client.BaseAddress = string.IsNullOrWhiteSpace(configuredUrl)
            ? sp.GetRequiredService<InternalApiBaseAddress>().Get()
            : new Uri(configuredUrl);
    });
}

builder.Services.AddScoped<AppStateServiceUser>();
builder.Services.AddScoped<AppStateServiceBPMS>();
builder.Services.AddScoped<AppStateServiceSIM>();
builder.Services.AddScoped<AppStateServiceOpenComex>();
builder.Services.AddScoped<AppStateServiceAsisComex>();
builder.Services.AddScoped<AppStateServiceExternalCarrier>();
builder.Services.AddScoped<AppStateServiceUsersSystem>();

// Detrás de un proxy / hosting (Docker, Nginx, Azure, Render...) respetar X-Forwarded-*
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// Diagnóstico de la carpeta de datos montada: avisa qué área no tiene sus archivos
var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("DataRoot");
startupLogger.LogInformation("Carpeta de datos (DataRoot): {DataRoot}", dataRoot);
foreach (var area in apiAreas)
{
    var areaDir = Path.Combine(dataRoot, area);
    if (!Directory.Exists(areaDir))
    {
        startupLogger.LogWarning("No existe la carpeta de datos del área {Area}: {Path}. Sus endpoints no tendrán datos.", area, areaDir);
    }
    else if (!File.Exists(Path.Combine(areaDir, "fields-config.json")))
    {
        startupLogger.LogWarning("Falta fields-config.json en {Path}.", areaDir);
    }
    else
    {
        startupLogger.LogInformation("Área {Area}: {Count} archivo(s) en {Path}", area, Directory.GetFiles(areaDir).Length, areaDir);
    }
}

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Las páginas de error de Blazor (/not-found) solo aplican a la Web, no a los endpoints /api
app.UseWhen(
    context => !context.Request.Path.StartsWithSegments("/api"),
    web => web.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true));

app.UseHttpsRedirection();

// Swagger de las APIs integradas: /swagger
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    foreach (var area in apiAreas)
    {
        options.SwaggerEndpoint($"/swagger/{area}/swagger.json", $"API {area}");
    }
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

// Endpoints de las APIs: api/{Area}/{Controller}/...
app.MapControllers();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Mapear la ruta del endpoint
app.MapHealthChecks("/health");

app.Run();
