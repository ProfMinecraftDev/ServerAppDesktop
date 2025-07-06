# Server App Desktop

Una aplicación de escritorio para Windows (**WinUI 3 sin MSIX**) diseñada para gestionar y monitorear servidores locales de forma sencilla e intuitiva.

---

## 📝 Descripción General

**Server App Desktop** nace de la necesidad de tener una herramienta centralizada para administrar aplicaciones de servidor que se ejecutan localmente. En lugar de depender de la línea de comandos para iniciar, detener y monitorear procesos, esta aplicación proporciona una interfaz gráfica moderna construida con **WinUI 3 desempaquetado**, sin necesidad de MSIX ni instalación a través del Store.

El proyecto está construido con C# y el **Windows App SDK**, siguiendo patrones de diseño modulares y modernos para asegurar que sea mantenible, escalable y portable.

---

## ✨ Características

*   **Configuración Inicial Guiada:** Un asistente para configurar parámetros del servidor en el primer inicio.
*   **Guardado de Configuración Persistente:** Ajustes almacenados en `settings.json` en `%APPDATA%`.
*   **Panel de Control Central:** UI principal con navegación clara entre módulos.
*   **Gestión de Procesos (Próximamente):** Control del proceso del servidor desde la UI.
*   **Visor de Archivos (Próximamente):** Exploración del directorio del servidor.
*   **Diseño Modular con Fluent UI:** Navegación adaptativa y visual estilo moderno.

---

## 🛠️ Stack Tecnológico

*   **Framework:** .NET 8 / Windows App SDK 1.7+
*   **UI:** WinUI 3 desempaquetado (sin MSIX)
*   **Lenguaje:** C#
*   **Serialización:** System.Text.Json para configuración
*   **Distribución:** `.exe` independiente mediante Inno Setup (sin Store)

---

## 🚀 Empezando

Requisitos:

* Visual Studio 2022 (v17.4+) con carga de trabajo "Desarrollo para Windows con SDK"
* .NET SDK 8 o superior
* Paquete NuGet: `Microsoft.WindowsAppSDK` versión 1.7+

### Pasos

1.  **Clona el repositorio:**
    ```powershell
    git clone https://github.com/ProfMinecraftDev/ServerAppDesktop.git
    ```
2.  **Abre la solución:**
    `ServerApp1Solution.sln` en Visual Studio.

3.  **Restauración automática o manual de NuGet**

4.  **Compila y ejecuta:**
    `F5` en modo depuración. Se genera un `.exe` desempaquetado.

---

## 📁 Estructura del Proyecto

* `/` — Archivos base: `App.xaml`, `MainWindow.xaml`, `Program.cs`
* `/Assets` — Recursos visuales
* `/Pages` — Vistas y módulos con `Frame` y navegación fluida
* `/Utils` — Configuración, helpers y lógica persistente (`SettingsManager.cs`)

---

## 🛣️ Roadmap

- [ ] Iniciar/detener `server.exe` desde la UI
- [ ] Indicador de estado del servidor en tiempo real
- [ ] Página de ajustes editable
- [ ] Visor de archivos tipo árbol
- [ ] Consola de logs integrada
- [ ] Fluent Design avanzado (sombra, color adaptable, animaciones)

---

## 🤝 Contribuciones

¡Bienvenidas! Sigue el flujo habitual de GitHub para PRs.

---

## 📄 Licencia

MIT — Consulta `LICENSE` para más detalles.
