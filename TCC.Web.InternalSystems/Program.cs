using TCC.Web.InternalSystems.Components;
using TCC.Web.InternalSystems.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Configuration.AddJsonFile("Data\\appusers.json", optional: false, reloadOnChange: true);

// Cambia la URL por el puerto real de tu API .NET 10 (ver launchSettings.json de la API)
builder.Services.AddHttpClient("BPMSApi", client =>
{
    client.BaseAddress = new Uri("https://localhost:44313/");
    //client.BaseAddress = new Uri("http://localhost:5080/");
});

builder.Services.AddHttpClient("SIMApi", client =>
{
    client.BaseAddress = new Uri("https://localhost:44314/");
});

builder.Services.AddHttpClient("OpenComexApi", client =>
{
    client.BaseAddress = new Uri("https://localhost:44315/");
});

builder.Services.AddHttpClient("AsisComexApi", client =>
{
    client.BaseAddress = new Uri("https://localhost:44316/");
});

builder.Services.AddHttpClient("ExternalCarrierApi", client =>
{
    client.BaseAddress = new Uri("https://localhost:44317/");
});

builder.Services.AddHttpClient("UsersSystemApi", client =>
{
    client.BaseAddress = new Uri("https://localhost:44318/");
});

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
