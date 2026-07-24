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
UBICACIONSELECCIONADA└── TCCWebApiCore    ├── Apis    └── Web```

---

### **Paso 3: Ubicar los scripts de automatización (.bat)**
Copie los archivos de control `StartApis_TCCWebApiCore.bat` y `StopApis_TCCWebApiCore.bat` directamente en la raíz de la ruta `UBICACIONSELECCIONADA\TCCWebApiCore`. 

Deben quedar ubicados al mismo nivel que las subcarpetas `Apis` y `Web`:

```text
UBICACIONSELECCIONADA\TCCWebApiCore├── Apis├── Web├── StartApis_TCCWebApiCore.bat
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
UBICACIONSELECCIONADA\TCCWebApiCore\Web\TCC.Web.InternalSystems```

---

### **Paso 6: Desplegar los paquetes de APIs**
Para todos los demás paquetes de API descargados:
1. Descomprima cada archivo `.zip`.
2. Cree una subcarpeta dentro de `UBICACIONSELECCIONADA\TCCWebApiCore\Apis\` con el nombre respectivo de cada API.
3. Coloque el contenido descomprimido en su subcarpeta correspondiente.

**Ejemplo de la estructura final esperada:**
```text
UBICACIONSELECCIONADA\TCCWebApiCore├── Apis│   ├── TCC.Api.Usuarios│   │   └── TCC.Api.Usuarios.dll
│   ├── TCC.Api.Clientes│   │   └── TCC.Api.Clientes.dll
│   └── TCC.Api.Envios│       └── TCC.Api.Envios.dll
├── Web│   └── TCC.Web.InternalSystems│       └── TCC.Web.InternalSystems.dll
├── StartApis_TCCWebApiCore.bat
└── StopApis_TCCWebApiCore.bat
```

---

## 🚀 Ejecución y Verificación

### **Paso 7: Iniciar los servicios**
Ejecute el archivo **`StartApis_TCCWebApiCore.bat`** (se recomienda hacer clic derecho y seleccionar *Ejecutar como administrador*).
* El script se encargará de levantar en segundo plano cada una de las aplicaciones y APIs configuradas.
* Durante el proceso, la consola irá mostrando en tiempo real la información de inicio y la verificación del estado *(Health Check)* de cada servicio.
* **Importante:** Al finalizar la validación, deje la ventana de comandos abierta para mantener las apis activas y poder realizar el monitoreo visual.

---

### **Paso 8: Monitoreo desde el Administrador de Tareas**
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

### **Paso 9: Bajar o detener las APIs**
Para detener la ejecución de todos los servicios asociados a la solución, puede optar por una de las siguientes opciones:

* **Opción A:** Cerrar la ventana de comandos del script `StartApis_TCCWebApiCore.bat`.
* **Opción B:** Ejecutar el archivo **`StopApis_TCCWebApiCore.bat`**, el cual enviará la señal de cierre a los procesos activos de manera controlada.

---
*Documentación generada para el repositorio TCCWebApiCore.*