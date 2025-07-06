@echo off
chcp 65001 > nul

:: Configuración
set CONFIG=Release
set ARCH=win-x64
set FRAMEWORK=net9.0-windows
set OUTDIR=.\bin\%CONFIG%\%FRAMEWORK%\%ARCH%\publish
set TARGET=.\Installer\Files
set LOG=build_log.txt

:: Paso 1: Navegar a la raíz del proyecto
cd "..\"

:: Paso 2: Limpiar carpeta de archivos anteriores
if exist "%TARGET%" (
    rd /s /q "%TARGET%"
)
md "%TARGET%"

:: Paso 3: Compilar el proyecto
echo Compilando proyecto con dotnet... >> %LOG%
dotnet publish -c %CONFIG% -r %ARCH% --self-contained true
if errorlevel 1 (
    echo Error durante la publicación. >> %LOG%
    exit /b
)

:: Paso 4: Validar carpeta de salida
if not exist "%OUTDIR%" (
    echo Error: La carpeta de salida no existe. >> %LOG%
    exit /b
)

:: Paso 5: Copiar archivos al instalador
echo Copiando archivos a "%TARGET%"... >> %LOG%
xcopy /s /y "%OUTDIR%" "%TARGET%"

:: Paso 6: Limpieza del proyecto
dotnet clean

:: Paso 7: Registro y notificación
echo [%date% %time%] Instalador generado correctamente. >> %LOG%
echo ✅ Creación de archivos finalizada. Revisa el log en %LOG%
pause
exit /b