#!/usr/bin/env bash
# Arma la carpeta Data que se monta en el contenedor Docker de TCC.Web.InternalSystems.
# Copia Data/ de cada API a <destino>/{Area}/ y el appusers.json de la Web a <destino>/.
#
#   ./Documents/PrepararCarpetaData.sh                    # -> ./docker-data
#   ./Documents/PrepararCarpetaData.sh /srv/tcc-data      # otra ruta
#   SOBRESCRIBIR=1 ./Documents/PrepararCarpetaData.sh     # reemplaza archivos existentes
set -euo pipefail

RAIZ="$(cd "$(dirname "$0")/.." && pwd)"
DESTINO="${1:-$RAIZ/docker-data}"

# Proyecto:Área (debe coincidir con ApiModule.Area)
APIS="TCC.Api.SystemOneBPMS:BPMS TCC.Api.SystemTwoSIM:SIM TCC.Api.SystemThreeOpenComex:OpenComex TCC.Api.SystemFourAsisComex:AsisComex External.Api.SystemCarrier:Carrier TCC.Api.SystemUsers:Users"

copiar() { # origen carpeta-destino
  mkdir -p "$2"
  local dst="$2/$(basename "$1")"
  if [[ -e "$dst" && -z "${SOBRESCRIBIR:-}" ]]; then echo "  [=] Ya existe, se conserva: $dst"; else cp "$1" "$dst"; echo "  [+] $dst"; fi
}

echo "Destino: $DESTINO"
copiar "$RAIZ/TCC.Web.InternalSystems/Data/appusers.json" "$DESTINO"
for par in $APIS; do
  proyecto="${par%%:*}"; area="${par##*:}"
  echo "$area  <-  $RAIZ/$proyecto/Data"
  for f in "$RAIZ/$proyecto/Data"/*.xlsx "$RAIZ/$proyecto/Data"/*.json; do
    [[ -f "$f" && "$(basename "$f")" != '~$'* ]] && copiar "$f" "$DESTINO/$area"
  done
done

# El contenedor corre como el usuario "app" (UID 1654) y escribe ApplicationDatabase_N.xlsx
echo
echo "Listo. En un servidor Linux dé permisos de escritura al contenedor:  sudo chown -R 1654:1654 \"$DESTINO\""
