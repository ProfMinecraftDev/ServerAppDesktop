![Server App Desktop hero image](docs/images/header.png)

<h1 align="center">
    Server App Desktop
</h1>

<p align="center">Una aplicación de escritorio avanzada para Windows construida con <b>WinUI 3/Windows App SDK 1.8</b> y <b>.NET 10,</b> diseñada para la gestión profesional de servidores Minecraft Bedrock y Minecraft Java con una interfaz moderna y rendimiento optimizado.</p>

<div align="center">

![Server App Desktop](https://img.shields.io/badge/Version-1.0.0.3%20(Preview)-darkblue?style=for-the-badge)
![.NET](https://img.shields.io/badge/.NET-10.0-purple?style=for-the-badge)
![WinUI](https://img.shields.io/badge/WinUI-3-blue?style=for-the-badge)
![Windows App SDK](https://img.shields.io/badge/Windows%20App%20SDK-1.8-green?style=for-the-badge)
![Plataforma](https://img.shields.io/badge/Plataforma-Windows%2010/11-lightgrey?style=for-the-badge)

</div>

---

## Arquitectura de la aplicación

### Diseño
- Server App Desktop comenzó desde su actualización 1.0 Preview 2 a utilizar WinUI 3/Windows App SDK, lo cual está ligado al Fluent Design System (Fluent 2) de Windows.
- Fluent 2 abre las puertas a mejor diseño y animaciones en la UI de la App sin afectar el rendimiento.
- Se está reconstruyendo todo el código de la Applicación con el patrón Modelo-Vista-VistaModelo (MVVM o Model-View-ViewModel).
- Se está comenzando a implementar el multilenguaje de la App, necesitamos que nos ayude a contribuir traduciendo los strings según el idioma que gustes.

### Código fuente
- Se está utilizando el nuevo SDK .NET 10.0 y C# 14 para mantener la Aplicación compatible con Windows 10 y 11.
- Se está utilizando la nueva Versión 1.8 de Windows App SDK, con soporte de desempaquetado (no MSIX).
- Se está organizando la App en proyectos separados (Ejecutable-Librería1-Libreria2-Libreria3) para mejor claridad en pruebas (sobre todo las unitarias).
- Utilizamos Dependency Injection (DI) del paquete `Microsoft.Extensions.Hosting` para mejor obtención de servicios.
- Reducimos cantidad de variables para evitar tanta sobrecarga en la memoria.

## Características

### Interfaz unificada
- Soporte de gestión de servidores Minecraft Bedrock y Minecraft Java.
- Archivo ejecutable de servidor (`server.jar` o `bedrock_server.exe`) corriendo de forma nativa en tu PC incluso sin conexión a internet.
- Envío de comandos al servidor (como tu queridísimo `say Hola Mundo!`).
- Monitoreo de rendimiento (CPU, RAM, Red, I/O del disco).
- Información de tu computadora a tu alcance sin abrir otras ventanas.
- Manejo de archivos de tu servidor (Borrar, Copiar, Mover, Renombrar, Backup y Edición).
- Envío de comentarios (feedback) para mejorar.
- Cumplimiento de normas Fluent Design.