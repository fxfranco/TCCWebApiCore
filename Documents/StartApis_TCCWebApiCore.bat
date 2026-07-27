@echo off
setlocal enabledelayedexpansion

:: ============================================================================
:: OBTENER LA RUTA DONDE ESTÁ GUARDADO ESTE .BAT
:: ============================================================================
set "RAIZ=%~dp0"

echo ===================================================
echo   Iniciando Servicios .NET Core
echo   Ruta Base: %RAIZ%
echo ===================================================
echo.

echo ===================================================
echo Version de .NET:
dotnet --version
echo ===================================================
echo.

:: ============================================================================
:: EJECUCIÓN DE CADA API
:: Formato de las instrucciones por cada API:
:: 1. Subcarpeta de la API
:: 2. Nombre del archivo .dll
:: 3. Puerto o Endpoint de prueba
:: ============================================================================

:: --- API 1: One BPMS ---
set "SUB_DIR=Apis\TCC.Api.SystemOneBPMS"
set "DLL_NAME=TCC.Api.SystemOneBPMS.dll"
set "URL_TEST1=https://localhost:44313/health"

echo [1/7] Lanzando API One BPMS...
if exist "%RAIZ%%SUB_DIR%\%DLL_NAME%" (
    start /b "" cmd /c "cd /d "%RAIZ%%SUB_DIR%" && dotnet "%DLL_NAME%"" > nul 2>&1
) else (
    echo   [ERROR] No se encontro el archivo: %RAIZ%%SUB_DIR%\%DLL_NAME%
)

:: --- API 2: Two SIM ---
set "SUB_DIR=Apis\TCC.Api.SystemTwoSIM"
set "DLL_NAME=TCC.Api.SystemTwoSIM.dll"
set "URL_TEST2=https://localhost:44314/health"

echo [2/7] Lanzando API Two SIM...
if exist "%RAIZ%%SUB_DIR%\%DLL_NAME%" (
    start /b "" cmd /c "cd /d "%RAIZ%%SUB_DIR%" && dotnet "%DLL_NAME%"" > nul 2>&1
) else (
    echo   [ERROR] No se encontro el archivo: %RAIZ%%SUB_DIR%\%DLL_NAME%
)

:: --- API 3: Three OpenComex ---
set "SUB_DIR=Apis\TCC.Api.SystemThreeOpenComex"
set "DLL_NAME=TCC.Api.SystemThreeOpenComex.dll"
set "URL_TEST3=https://localhost:44315/health"

echo [3/7] Lanzando API Three OpenComex...
if exist "%RAIZ%%SUB_DIR%\%DLL_NAME%" (
    start /b "" cmd /c "cd /d "%RAIZ%%SUB_DIR%" && dotnet "%DLL_NAME%"" > nul 2>&1
) else (
    echo   [ERROR] No se encontro el archivo: %RAIZ%%SUB_DIR%\%DLL_NAME%
)

:: --- API 4: Four AsisComex ---
set "SUB_DIR=Apis\TCC.Api.SystemFourAsisComex"
set "DLL_NAME=TCC.Api.SystemFourAsisComex.dll"
set "URL_TEST4=https://localhost:44316/health"

echo [4/7] Lanzando API Four AsisComex...
if exist "%RAIZ%%SUB_DIR%\%DLL_NAME%" (
    start /b "" cmd /c "cd /d "%RAIZ%%SUB_DIR%" && dotnet "%DLL_NAME%"" > nul 2>&1
) else (
    echo   [ERROR] No se encontro el archivo: %RAIZ%%SUB_DIR%\%DLL_NAME%
)

:: --- API 5: Five SystemCarrier ---
set "SUB_DIR=Apis\External.Api.SystemCarrier"
set "DLL_NAME=External.Api.SystemCarrier.dll"
set "URL_TEST5=https://localhost:44317/health"

echo [5/7] Lanzando API Five SystemCarrier...
if exist "%RAIZ%%SUB_DIR%\%DLL_NAME%" (
    start /b "" cmd /c "cd /d "%RAIZ%%SUB_DIR%" && dotnet "%DLL_NAME%"" > nul 2>&1
) else (
    echo   [ERROR] No se encontro el archivo: %RAIZ%%SUB_DIR%\%DLL_NAME%
)

:: --- API 6: Six SystemUsers ---
set "SUB_DIR=Apis\TCC.Api.SystemUsers"
set "DLL_NAME=TCC.Api.SystemUsers.dll"
set "URL_TEST6=https://localhost:44318/health"

echo [6/7] Lanzando API Six SystemUsers...
if exist "%RAIZ%%SUB_DIR%\%DLL_NAME%" (
    start /b "" cmd /c "cd /d "%RAIZ%%SUB_DIR%" && dotnet "%DLL_NAME%"" > nul 2>&1
) else (
    echo   [ERROR] No se encontro el archivo: %RAIZ%%SUB_DIR%\%DLL_NAME%
)

:: --- API 7: Seven Web System ---
set "SUB_DIR=Web\TCC.Web.InternalSystems"
set "DLL_NAME=TCC.Web.InternalSystems.dll"
set "URL_TEST7=http://localhost:5000/health"

echo [7/7] Lanzando API Seven Web System...
if exist "%RAIZ%%SUB_DIR%\%DLL_NAME%" (
    start /b "" cmd /c "cd /d "%RAIZ%%SUB_DIR%" && dotnet "%DLL_NAME%"" > nul 2>&1
) else (
    echo   [ERROR] No se encontro el archivo: %RAIZ%%SUB_DIR%\%DLL_NAME%
)

:: ============================================================================
:: TIEMPO DE ESPERA Y COMPROBACIÓN
:: ============================================================================
echo.
echo Esperando 20 segundos a que los procesos inicien...
timeout /t 20 /nobreak > nul
echo.

echo ===================================================
echo   Verificando Conexion a las APIs
echo ===================================================

call :VerificarAPI "API One BPMS" "%URL_TEST1%"
call :VerificarAPI "API Two SIM" "%URL_TEST2%"
call :VerificarAPI "API Three OpenComex" "%URL_TEST3%"
call :VerificarAPI "API Four AsisComex" "%URL_TEST4%"
call :VerificarAPI "API Five SystemCarrier" "%URL_TEST5%"
call :VerificarAPI "API Six SystemUsers" "%URL_TEST6%"
call :VerificarAPI "API Seven Web" "%URL_TEST7%"

echo.
echo ===================================================
echo Proceso finalizado.
echo ===================================================
pause
goto :eof


:: ============================================================================
:: FUNCIÓN DE VERIFICACIÓN HTTP (PowerShell)
:: ============================================================================
:VerificarAPI
set "NOMBRE_API=%~1"
set "URL_API=%~2"

powershell -Command "$ProgressPreference = 'SilentlyContinue'; try { $res = Invoke-WebRequest -Uri '%URL_API%' -UseBasicParsing -TimeoutSec 4; if ($res.StatusCode -ge 200 -and $res.StatusCode -lt 400) { exit 0 } else { exit 1 } } catch { exit 1 }"

if %errorlevel% equ 0 (
    echo [ OK ] %NOMBRE_API% esta respondiendo en: %URL_API%
) else (
    echo [ERROR] %NOMBRE_API% NO responde en: %URL_API%
)
goto :eof