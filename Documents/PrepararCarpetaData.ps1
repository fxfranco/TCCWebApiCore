<#
.SYNOPSIS
  Arma la carpeta Data que se monta en el contenedor Docker de TCC.Web.InternalSystems.

.DESCRIPTION
  Copia los Excel y JSON de la carpeta Data de cada API a <Destino>\{Area}\ y el
  appusers.json de la Web a <Destino>\. Esa carpeta se sube al hosting y se monta
  como /app/Data (ver docker-compose.yml -> DATA_PATH). La imagen no la contiene.

.EXAMPLE
  .\Documents\PrepararCarpetaData.ps1
  .\Documents\PrepararCarpetaData.ps1 -Destino D:\Servidor\tcc-data -Sobrescribir
#>
param(
    [string]$Destino = (Join-Path (Split-Path $PSScriptRoot -Parent) "docker-data"),
    [switch]$Sobrescribir
)

$ErrorActionPreference = "Stop"
$raiz = Split-Path $PSScriptRoot -Parent

# Proyecto de API -> Área (debe coincidir con ApiModule.Area)
$apis = [ordered]@{
    "TCC.Api.SystemOneBPMS"        = "BPMS"
    "TCC.Api.SystemTwoSIM"         = "SIM"
    "TCC.Api.SystemThreeOpenComex" = "OpenComex"
    "TCC.Api.SystemFourAsisComex"  = "AsisComex"
    "External.Api.SystemCarrier"   = "Carrier"
    "TCC.Api.SystemUsers"          = "Users"
}

New-Item -ItemType Directory -Force -Path $Destino | Out-Null

function Copiar($origen, $carpetaDestino) {
    New-Item -ItemType Directory -Force -Path $carpetaDestino | Out-Null
    $archivo = Join-Path $carpetaDestino (Split-Path $origen -Leaf)
    if ((Test-Path $archivo) -and -not $Sobrescribir) {
        Write-Host "  [=] Ya existe, se conserva: $archivo"
    } else {
        Copy-Item $origen $archivo -Force
        Write-Host "  [+] $archivo"
    }
}

Write-Host "Destino: $Destino"
Copiar (Join-Path $raiz "TCC.Web.InternalSystems\Data\appusers.json") $Destino

foreach ($api in $apis.GetEnumerator()) {
    $origenData = Join-Path $raiz "$($api.Key)\Data"
    Write-Host "$($api.Value)  <-  $origenData"
    Get-ChildItem $origenData -File | Where-Object { $_.Extension -in ".xlsx", ".json" -and -not $_.Name.StartsWith('~$') } |
        ForEach-Object { Copiar $_.FullName (Join-Path $Destino $api.Value) }
}

Write-Host ""
Write-Host "Listo. Suba la carpeta '$Destino' al servidor y móntela como /app/Data (DATA_PATH en .env)."
