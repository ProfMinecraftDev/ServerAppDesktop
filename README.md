![Server App Desktop hero image](docs/images/Header.png)

<h1 align="center">
    Server App Desktop
</h1>

<p align="center">Una aplicación de escritorio avanzada para Windows construida con <b>WinUI 3/Windows App SDK 1.8</b> y <b>.NET 10,</b> diseñada para la gestión profesional de servidores Minecraft Bedrock y Minecraft Java con una interfaz moderna y rendimiento optimizado.</p>

<div align="center">

![Server App Desktop](https://img.shields.io/badge/Versión-1.0.0.3%20(Preview)-darkblue?style=for-the-badge)
![.NET](https://img.shields.io/badge/.NET-10.0-purple?style=for-the-badge)
![WinUI](https://img.shields.io/badge/WinUI-3-blue?style=for-the-badge)
![Windows App SDK](https://img.shields.io/badge/Windows%20App%20SDK-1.8-green?style=for-the-badge)
![Plataforma](https://img.shields.io/badge/Plataforma-Windows%2010/11-lightgrey?style=for-the-badge)

</div>

---

## Arquitectura de la aplicación

### Diseño
- A partir de su actualización 1.0 Preview 2, Server App Desktop comenzó a utilizar WinUI 3/Windows App SDK, lo cual está ligado al Fluent Design System (Fluent 2) de Windows.
- Fluent 2 abre las puertas a un mejor diseño y animaciones en la interfaz de usuario (UI) de la aplicación sin afectar el rendimiento.
- Se está reconstruyendo todo el código de la aplicación bajo el patrón **Modelo-Vista-VistaModelo** (MVVM o Model-View-ViewModel).
- Se ha comenzado a implementar el soporte multiidioma; necesitamos tu ayuda para contribuir traduciendo las cadenas de texto (strings) según el idioma que prefieras.

### Código fuente
- Se está utilizando el nuevo SDK **.NET 10.0** y **C# 14** para mantener la aplicación compatible con Windows 10 y 11.
- Se utiliza la versión 1.8 de Windows App SDK, con soporte para ejecución sin empaquetar (no MSIX).
- La aplicación se organiza en proyectos separados (Ejecutable-Librería1-Librería2-Librería3) para una mejor claridad en las pruebas (especialmente las unitarias).
- Utilizamos Inyección de Dependencias (DI) del paquete `Microsoft.Extensions.Hosting` para una mejor obtención de servicios.
- Reducimos la cantidad de variables para evitar sobrecarga en la memoria.
- **Estructura de directorios:**
    ```text
    ServerAppDesktop
    |   ServerAppDesktop.sln            # Solución
    |
    +---installer                       
    |   \---Assets                      # Archivos del instalador
    \---src
        +---ServerAppDesktop            # Proyecto principal, UI y ViewModels
        +---ServerAppDesktop.Controls   # Controles personalizados
        +---ServerAppDesktop.Helpers    # Ayudantes (Helpers)
        +---ServerAppDesktop.Models     # Modelos (datos puros)
        +---ServerAppDesktop.Converters # Conversores (Converters)
        \---ServerAppDesktop.Services   # Servicios (Lógica completa)
    ```

## Características

### Interfaz unificada
- Soporte para la gestión de servidores Minecraft Bedrock y Minecraft Java.
- Ejecución nativa del archivo ejecutable del servidor (`server.jar` o `bedrock_server.exe`) en tu computadora, incluso sin conexión a internet.
- Envío de comandos al servidor (como el clásico `say ¡Hola Mundo!`).
- Monitoreo de rendimiento en tiempo real (CPU, RAM, Red, E/S de disco).
- Información de tu computadora al alcance de tu mano sin abrir otras ventanas.
- Gestión de archivos de tu servidor (Eliminar, Copiar, Mover, Renombrar, Respaldos y Edición).
- Envío de comentarios (feedback) para mejorar la aplicación.
- Cumplimiento de las normas de Fluent Design.

### Configuración
- Todo se guarda en archivos JSON con un formato legible.
- Configuración persistente en `%LocalAppData%\Server App Desktop\Settings\Settings.json` (editable).
- Soporte para instalación por usuario (Per-User) y por equipo (Per-Machine).

## Instalación y Configuración

### Prerrequisitos
- **SO**: Windows 10 Versión 2004 (19041) o superior.
- **Hardware**: Arquitecturas x64, x86 o ARM64.
- **Visual Studio**: 2022 versión 17.8+ con la carga de trabajo "Desarrollo de aplicaciones WinUI".

### Instalación desde Release
1. **Descarga el instalador**: `ServerAppDesktop-Setup-1.0.0.3-Preview.exe`
2. **Ejecuta como administrador** para una instalación completa **(NO ES OBLIGATORIO)**.
3. **Sigue el asistente** de configuración inicial al arrancar por primera vez.

### Compilación desde Código Fuente

```powershell
# 1. Clonar el repositorio
git clone [https://github.com/ProfMinecraftDev/ServerAppDesktop.git](https://github.com/ProfMinecraftDev/ServerAppDesktop.git)
cd ServerAppDesktop

# 2. Restaurar dependencias
dotnet restore ServerAppDesktop.sln

# 3. Compilar en modo Debug
dotnet build ServerAppDesktop.sln -c Debug

# 4. Compilar para Release (con optimizaciones)
dotnet publish ServerAppDesktop.sln -c Release -r win-x64 --self-contained

---
```

## Idiomas de la documentación
Esta documentación también está disponible en los siguientes idiomas:

| Idioma | Estado | Enlace |
| :--- | :---: | :--- |
| ![English](https://img.shields.io/badge/English-en--US-blue) | **Completo** | [docs/en-US/README.md](./docs/en-US/README.md) |

> [!TIP]
> ¿Quieres ayudar? Si hablas otro idioma, ¡tus contribuciones para traducir la documentación son bienvenidas!