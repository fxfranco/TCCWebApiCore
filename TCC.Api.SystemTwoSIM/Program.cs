using Microsoft.OpenApi.Models;
using TCC.Api.SystemTwoSIM.Extensions;
using TCC.Api.SystemTwoSIM.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Registrar los servicios de salud
builder.Services.AddHealthChecks();

// Add services to the container.

builder.Services.AddControllers();

// Composition root: servicios de aplicación + autenticación JWT
builder.Services.AddApplicationServices();
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Configuration.AddJsonFile("Data\\appusers.json", optional: false, reloadOnChange: true);

// Registrar como Singleton para que la memoria sea compartida de forma global
builder.Services.AddSingleton<InMemoryExcelStorage>();

// Swagger / OpenAPI con soporte para autorización Bearer
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Api System Two SIM JWT API",
        Version = "v1",
        Description = "API de ejemplo: JWT + extracción de campos de Excel usando SOLID."
    });

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
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Mapear la ruta del endpoint
app.MapHealthChecks("/health");

app.Run();
