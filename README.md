# TCCWebApiCore
Proyecto de simulación de los multiples sistemas web y api que se necesitan

---

# Guía de Instalación y Despliegue - TCCWebApiCore

Este documento contiene las instrucciones detalladas paso a paso para la instalación, despliegue, verificación y detención de la suite de APIs y aplicaciones web del proyecto **TCCWebApiCore** en entornos de Windows.

---

## 📋 Requisitos Previos

Antes de iniciar con el proceso de despliegue, asegúrese de contar con los permisos de administrador en la máquina de destino y conexión a Internet para la descarga de dependencias.

---

## 🛠️ Pasos de Instalación

### **Paso 1: Descargar e instalar el SDK de .NET**
Descargue e instale el **SDK x64 de .NET en su versión .NET 10** en la máquina donde se ejecutarán los servicios.
* **Enlace de descarga:** [https://dotnet.microsoft.com/es-es/download](https://dotnet.microsoft.com/es-es/download)

---

### **Paso 2: Crear la estructura de carpetas**
Elija una ubicación en su sistema (por ejemplo, `C:\Servicios` o `D:\Publicaciones`) donde se instalarán las APIs y aplicaciones web. Dentro de la ubicación seleccionada, cree la siguiente estructura jerárquica de carpetas:

```text
UBICACIONSELECCIONADA\
└── TCCWebApiCore\
    ├── Apis\
    └── Web\
```

---

### **Paso 3: Ubicar los scripts de automatización (.bat)**
En el repositorio en la carpeta `Documents` descargue y copie los archivos de control `StartApis_TCCWebApiCore.bat` y `StopApis_TCCWebApiCore.bat` directamente en la raíz de la ruta `UBICACIONSELECCIONADA\TCCWebApiCore`. 

Deben quedar ubicados al mismo nivel que las subcarpetas `Apis` y `Web`:

```text
UBICACIONSELECCIONADA\TCCWebApiCore\
├── Apis\
├── Web\
├── StartApis_TCCWebApiCore.bat
└── StopApis_TCCWebApiCore.bat
```
---

### **Paso 4: Descargar los paquetes de publicación desde GitHub Actions**
Acceda al repositorio de GitHub Actions para descargar los artefactos (archivos `.zip`) de cada API publicados en la última ejecución exitosa del flujo de trabajo:
* **URL de descarga:** [https://github.com/fxfranco/TCCWebApiCore/actions/workflows/CITCCWebApiCoreManual.yml](https://github.com/fxfranco/TCCWebApiCore/actions/workflows/CITCCWebApiCoreManual.yml)

> **Nota:** Ingrese a la última ejecución del flujo *(workflow run)* y diríjase a la sección de **Artifacts** al final de la página para descargar los archivos zip correspondiente a cada aplicación.

---

### **Paso 5: Desplegar el paquete Web**
1. Ubique el paquete comprimido `TCC.Web.InternalSystems-build.zip` correspondiente a la API/Aplicación **TCC.Web.InternalSystems**.
2. Descomprima todo su contenido.
3. Ubique los archivos descomprimidos dentro de la carpeta `Web`, alojados en una nueva subcarpeta con el mismo nombre del componente:

```text
UBICACIONSELECCIONADA\TCCWebApiCore\Web\TCC.Web.InternalSystems\
```

---

### **Paso 6: Desplegar los paquetes de APIs**
Para todos los demás paquetes de API descargados:
1. Descomprima cada archivo `.zip`.
2. Cree una subcarpeta dentro de `UBICACIONSELECCIONADA\TCCWebApiCore\Apis\` con el nombre respectivo de cada API.
3. Coloque el contenido descomprimido en su subcarpeta correspondiente.

**Ejemplo de la estructura final esperada:**

```text
UBICACIONSELECCIONADA\TCCWebApiCore\
├── Apis\
│   ├── TCC.Api.Usuarios\
│   │   └── TCC.Api.Usuarios.dll
│   ├── TCC.Api.Clientes\
│   │   └── TCC.Api.Clientes.dll
│   └── TCC.Api.Envios\
│       └── TCC.Api.Envios.dll
├── Web\
│   └── TCC.Web.InternalSystems\
│       └── TCC.Web.InternalSystems.dll
├── StartApis_TCCWebApiCore.bat
└── StopApis_TCCWebApiCore.bat
```

---
## 🚀 Organizar archivos de bases de datos

### **Paso 7: Ubicar los archivos de base de datos de Excel**
1. Los 6 archivos de excel previamente enviados se deben copiar y ubicar cada uno en la siguiente ubicación de cada api de la siguiente manera

```text
1_BPMS.xlsx ---> TCC.Api.SystemOneBPMS\Data
2_SIM.xlsx --> TCC.Api.SystemTwoSIM\Data
3_OpenComex.xlsx --> TCC.Api.SystemThreeOpenComex\Data
4_AsisComex.xlsx --> TCC.Api.SystemFourAsisComex\Data
5_Transportista.xlsx --> External.Api.SystemCarrier\Data
6_Users.xlsx --> TCC.Api.SystemUsers\Data
```

2. Después de copiar cada archivo en su ubicación, todos los archivos deben cambiarse por el siguiente nombre `ApplicationDatabase.xlsx`.

---

## 🚀 Ejecución y Verificación

### **Paso 8: Iniciar los servicios**
Ejecute el archivo **`StartApis_TCCWebApiCore.bat`** (se recomienda hacer clic derecho y seleccionar *Ejecutar como administrador*).
* El script se encargará de levantar en segundo plano cada una de las aplicaciones y APIs configuradas.
* Durante el proceso, la consola irá mostrando en tiempo real la información de inicio y la verificación del estado *(Health Check)* de cada servicio.
* **Importante:** Al finalizar la validación, deje la ventana de comandos abierta para mantener las apis activas y poder realizar el monitoreo visual.

---

### **Paso 9: Monitoreo desde el Administrador de Tareas**
Para verificar qué APIs se están ejecutando en el sistema mediante la interfaz de Windows:
1. Abra el **Administrador de Tareas** (`Ctrl + Shift + Esc`).
2. Diríjase a la pestaña **Detalles**.
3. Busque los procesos ejecutados bajo el nombre **`dotnet.exe`**.
4. Para saber a qué API exacta corresponde cada proceso `dotnet.exe`:
   * Haga clic derecho sobre cualquier encabezado de columna (ej. *Nombre* o *PID*).
   * Seleccione **Seleccionar columnas**.
   * Active la casilla **Línea de comandos**.
   * En la columna recién activada podrá visualizar la ruta completa y el archivo `.dll` asociado a cada API en ejecución.

---

## 🛑 Detención de Servicios

### **Paso 10: Bajar o detener las APIs**
Para detener la ejecución de todos los servicios asociados a la solución, puede optar por una de las siguientes opciones:

* **Opción A:** Cerrar la ventana de comandos del script `StartApis_TCCWebApiCore.bat`.
* **Opción B:** Ejecutar el archivo **`StopApis_TCCWebApiCore.bat`**, el cual enviará la señal de cierre a los procesos activos de manera controlada.

---

# 🧩 Modo integrado: una sola aplicación (Web + 6 APIs)

`TCC.Web.InternalSystems` referencia los 6 proyectos de API y los ejecuta **dentro del mismo proceso**. Para publicar en un hosting solo se sube la Web; los endpoints de cada API quedan expuestos por ella, separados por área:

| API | Proyecto | Área | Ejemplo de endpoint |
|-----|----------|------|---------------------|
| BPMS | TCC.Api.SystemOneBPMS | `BPMS` | `POST /api/BPMS/auth/login` |
| SIM | TCC.Api.SystemTwoSIM | `SIM` | `GET /api/SIM/excelfields/latest-data` |
| OpenComex | TCC.Api.SystemThreeOpenComex | `OpenComex` | `POST /api/OpenComex/excelfields/extract` |
| AsisComex | TCC.Api.SystemFourAsisComex | `AsisComex` | `POST /api/AsisComex/excelfields/save-in-memory` |
| Transportista externo | External.Api.SystemCarrier | `Carrier` | `GET /api/Carrier/excelfields/download-latest` |
| Usuarios | TCC.Api.SystemUsers | `Users` | `POST /api/Users/excelfields/clear-server-memory` |

* **Swagger** de todas las APIs: `/swagger` (un documento por área). **Health check**: `/health`.
* **JWT**: cada API tiene su propio esquema (`Bearer_{Area}`) y su configuración en `appsettings.json` → `Apis:{Area}:Jwt`. Un token de BPMS no sirve en SIM (igual que cuando eran APIs separadas).
* **Excel / JSON**: cada API lee y escribe en `Data/{Area}/` dentro de la Web publicada (se copian al compilar desde la carpeta `Data` de cada API). Al iniciar, cada API carga su Excel en memoria (`ExcelInitializerServices`).
* **Páginas Razor**: siguen usando `IHttpClientFactory`; los clientes de `ApiSettings` vacíos apuntan a la misma aplicación (URL autodetectada o `InternalApi:BaseUrl`). Si se pone una URL absoluta en `ApiSettings`, se usa esa API externa.
* Cada API **sigue funcionando sola** con su `Program.cs` (scripts `.bat`), ahora en rutas `api/{Area}/...`.

# 🐳 Docker y Docker Hub

Archivos: `TCC.Web.InternalSystems/Dockerfile`, `docker-compose.yml`, `.dockerignore`, `.env.example` (todo desde la raíz de la solución).

## Carpeta Data dentro de la imagen

Por defecto la imagen **incluye** la carpeta Data en `/app/Data` (pensado para hostings sin persistencia). Al construir se toman:

* `TCC.Web.InternalSystems/Data/appusers.json` → `/app/Data/appusers.json`
* `<Proyecto API>/Data/*.xlsx|*.json` → `/app/Data/{BPMS|SIM|OpenComex|AsisComex|Carrier|Users}/`

El build muestra la lista de archivos incluidos y falla si falta `appusers.json`.

* **Cambiar datos**: editar el Excel/JSON en la carpeta `Data` de la API correspondiente, reconstruir y volver a subir la imagen.
* **Sin persistencia**: lo que se guarde con "Guardar en Sistema" (`ApplicationDatabase_N.xlsx`) existe solo mientras viva el contenedor; al reiniciarse/recrearse vuelve a los datos de la imagen.
* **Imagen sin datos** (si algún hosting sí permite montar carpetas): `docker build --build-arg INCLUDE_DATA=false ...` y montar la carpeta en `/app/Data` (se genera con `Documents/PrepararCarpetaData.ps1` / `.sh`; ver el bloque `volumes` comentado en `docker-compose.yml`).
* En Visual Studio / `dotnet run` no cambia nada: los archivos se copian a `bin\...\Data` al compilar.

## Construir, probar y publicar

```bash
# 1. Variables: copiar .env.example a .env y ajustar DOCKERHUB_USER, JWT_SECRET_KEY, HOST_PORT
# 2. Construir y levantar (la imagen ya trae la carpeta Data)  ->  http://localhost:8080
docker compose up -d --build
docker compose logs -f

# 3. Subir a Docker Hub
docker login
docker compose push
#    o, sin compose:
docker build -f TCC.Web.InternalSystems/Dockerfile -t <usuario>/tcc-internal-systems:latest .
docker push <usuario>/tcc-internal-systems:latest

# 4. En el servidor / hosting (sin volúmenes)
docker run -d -p 8080:8080 --name tcc-internal-systems <usuario>/tcc-internal-systems:latest

# 5. Después de cambiar un Excel o JSON en el repositorio: reconstruir y volver a subir
docker compose build && docker compose push
```

* El contenedor escucha en **HTTP 8080**; el HTTPS lo pone el hosting / proxy (se respetan los encabezados `X-Forwarded-*`).
* Para producción cambie la clave JWT con `JWT_SECRET_KEY` (o `Apis__{Area}__Jwt__SecretKey`).
