# =============================================================================
# TCC.Web.InternalSystems  (Web Razor/Blazor + las 6 APIs integradas)
# Contexto de build: la RAIZ de la solución (donde está TCC.WebApiCore.slnx)
#
#   docker build -f TCC.Web.InternalSystems/Dockerfile -t <usuario>/tcc-internal-systems:latest .
#
# La carpeta Data (Excel + JSON de cada API) se INCLUYE en la imagen (/app/Data), para
# hostings sin persistencia. Para generar una imagen sin datos (montándolos aparte):
#   docker build --build-arg INCLUDE_DATA=false ...
# =============================================================================

# ---------- Etapa 1: compilación ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG INCLUDE_DATA=true
WORKDIR /src

# 1) Solo los .csproj primero => la capa de "restore" queda en caché mientras no cambien los paquetes
COPY TCC.Web.InternalSystems/TCC.Web.InternalSystems.csproj           TCC.Web.InternalSystems/
COPY TCC.Api.SystemOneBPMS/TCC.Api.SystemOneBPMS.csproj               TCC.Api.SystemOneBPMS/
COPY TCC.Api.SystemTwoSIM/TCC.Api.SystemTwoSIM.csproj                 TCC.Api.SystemTwoSIM/
COPY TCC.Api.SystemThreeOpenComex/TCC.Api.SystemThreeOpenComex.csproj TCC.Api.SystemThreeOpenComex/
COPY TCC.Api.SystemFourAsisComex/TCC.Api.SystemFourAsisComex.csproj   TCC.Api.SystemFourAsisComex/
COPY External.Api.SystemCarrier/External.Api.SystemCarrier.csproj     External.Api.SystemCarrier/
COPY TCC.Api.SystemUsers/TCC.Api.SystemUsers.csproj                   TCC.Api.SystemUsers/
# SelfContained=false: en el contenedor el runtime lo aporta la imagen base aspnet:10.0
# (evita NETSDK1067 si algún csproj trae <SelfContained>true</SelfContained> para otras publicaciones).
RUN dotnet restore TCC.Web.InternalSystems/TCC.Web.InternalSystems.csproj /p:SelfContained=false

# 2) Resto del código y publicación
# Sin --no-restore: el restore anterior solo ve los .csproj y, al no haber archivos .razor,
# el SDK de .NET 10 omite el paquete Microsoft.AspNetCore.App.Internal.Assets (que trae
# _framework/blazor.web.js). Restaurar de nuevo aquí, con todo el código, lo incluye
# (los demás paquetes ya quedan en caché de la capa anterior, así que es rápido).
COPY . .
RUN dotnet publish TCC.Web.InternalSystems/TCC.Web.InternalSystems.csproj \
      -c $BUILD_CONFIGURATION \
      -o /app/publish \
      /p:UseAppHost=false \
      /p:SelfContained=false \
      /p:PublishDataFiles=$INCLUDE_DATA

# Verificación: el script de Blazor debe estar en la publicación
RUN test -f /app/publish/wwwroot/_framework/blazor.web.js || { echo "ERROR: falta wwwroot/_framework/blazor.web.js"; exit 1; }

# Verificación: si se pidieron los datos, deben estar en la publicación
RUN if [ "$INCLUDE_DATA" = "true" ]; then \
      test -f /app/publish/Data/appusers.json || { echo "ERROR: Data/appusers.json no quedó en la publicación"; exit 1; }; \
      echo "Archivos de datos incluidos en la imagen:"; find /app/publish/Data -type f | sort; \
    fi

# ---------- Etapa 2: runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DataRoot=/app/Data

# /app/Data (dentro de la imagen):
#   appusers.json
#   {BPMS|SIM|OpenComex|AsisComex|Carrier|Users}/ApplicationDatabase.xlsx, fields-config.json, ...
# El usuario "app" (no root) debe poder escribir ahí: "Guardar en Sistema" crea
# ApplicationDatabase_N.xlsx. Sin volumen, esos archivos nuevos se pierden al recrear el contenedor.
COPY --from=build --chown=app:app /app/publish .
RUN mkdir -p /app/Data && chown app:app /app/Data

USER app
EXPOSE 8080

ENTRYPOINT ["dotnet", "TCC.Web.InternalSystems.dll"]
