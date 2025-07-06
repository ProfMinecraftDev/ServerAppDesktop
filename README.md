# Server App Desktop

Una aplicación de escritorio para Windows (WPF) diseñada para gestionar y monitorear servidores locales de forma sencilla e intuitiva.

---


## 📝 Descripción General

**Server App Desktop** nace de la necesidad de tener una herramienta centralizada para administrar aplicaciones de servidor que se ejecutan localmente. En lugar de depender de la línea de comandos para iniciar, detener y monitorear procesos, esta aplicación proporciona una interfaz gráfica moderna para realizar estas tareas.

El proyecto está construido con C# y WPF, siguiendo patrones de diseño modernos para asegurar que sea mantenible y escalable.

---

## ✨ Características

*   **Configuración Inicial Guiada:** Un asistente de bienvenida para configurar los parámetros del servidor la primera vez que se ejecuta la aplicación.
*   **Guardado de Configuración Persistente:** Los ajustes se guardan en un archivo `settings.json` en la carpeta de datos del usuario (`%APPDATA%`), eliminando la necesidad de reconfigurar en cada inicio.
*   **Panel de Control Central:** Una interfaz principal con navegación clara para acceder a las diferentes secciones de la aplicación.
*   **Gestión de Procesos (Próximamente):** Funcionalidad para iniciar, detener y monitorear el estado del proceso del servidor directamente desde la UI.
*   **Visor de Archivos (Próximamente):** Una sección para explorar los archivos y carpetas del directorio del servidor.

---

## 🛠️ Stack Tecnológico

*   **Framework:** .NET / .NET Core
*   **UI:** Windows Presentation Foundation (WPF)
*   **Lenguaje:** C#
*   **Serialización:** System.Text.Json para manejar la configuración.

---

## 🚀 Empezando

Para compilar y ejecutar este proyecto localmente, necesitarás:

*   Visual Studio 2022 (o superior) con la carga de trabajo ".NET desktop development".
*   .NET SDK (la versión se puede encontrar en el archivo `.csproj`).

### Pasos para la Instalación

1.  **Clona el repositorio:**
    ```powershell
    git clone https://github.com/ProfMinecraftDev/ServerAppDesktop.git
    ```
2.  **Abre la solución:**
    Navega a la carpeta del proyecto y abre el archivo `ServerApp1Solution.sln` con Visual Studio.

3.  **Restaura las dependencias:**
    Visual Studio debería restaurar los paquetes NuGet automáticamente. Si no, haz clic derecho en la solución en el "Solution Explorer" y selecciona "Restore NuGet Packages".

4.  **Compila y ejecuta:**
    Presiona `F5` o el botón "Start" para compilar y ejecutar la aplicación en modo de depuración.

---

## 📁 Estructura del Proyecto

El código está organizado de la siguiente manera para facilitar su mantenimiento:

*   `/` - **Raíz del Proyecto:** Contiene los archivos principales de la aplicación como `App.xaml` y `MainWindow.xaml`.
*   `/Assets` - **Recursos:** Iconos, imágenes y otros recursos estáticos.
*   `/Pages` - **Páginas de Navegación:** Contiene las diferentes vistas (páginas XAML) que se muestran en el `Frame` de la `MainWindow`.
*   `/Utils` - **Utilidades:** Clases de ayuda y lógica de negocio, como `SettingsManager.cs` para gestionar la configuración y `FirstStartApp.xaml` para la configuración inicial.

---

## 🛣️ Roadmap del Proyecto

El objetivo es seguir mejorando la aplicación. Las próximas grandes funcionalidades planeadas son:

-   [ ] **Gestión de Procesos:** Implementar la lógica para iniciar y detener el `server.exe`.
-   [ ] **Monitor de Estado:** Mostrar en tiempo real si el servidor está `Online` o `Offline`.
-   [ ] **Página de Configuración:** Permitir al usuario modificar los ajustes sin tener que borrar el archivo de configuración.
-   [ ] **Visor de Archivos:** Implementar una vista de árbol para navegar por los archivos del servidor.
-   [ ] **Consola de Salida:** Añadir una vista para capturar y mostrar la salida estándar (logs) del proceso del servidor.
-   [ ] **Mejoras en la UI/UX:** Refinar estilos y la experiencia de usuario general.

---

## 🤝 Contribuciones

¡Las contribuciones son bienvenidas! Si quieres ayudar a mejorar el proyecto, por favor sigue estos pasos:

1.  **Haz un Fork** del repositorio.
2.  **Crea una nueva rama** para tu funcionalidad (`git checkout -b feature/AmazingFeature`).
3.  **Haz tus cambios** y haz commit (`git commit -m 'Add some AmazingFeature'`).
4.  **Haz Push** a tu rama (`git push origin feature/AmazingFeature`).
5.  **Abre un Pull Request**.

---

## 📄 Licencia

Este proyecto está distribuido bajo la Licencia MIT. Consulta el archivo `LICENSE` para más detalles.
