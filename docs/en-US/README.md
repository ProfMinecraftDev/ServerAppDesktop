![Server App Desktop hero image](/docs/images/Header.png)

<h1 align="center">
    Server App Desktop
</h1>

<p align="center">An advanced Windows desktop application built with <b>WinUI 3/Windows App SDK 1.8</b> and <b>.NET 10,</b> designed for professional management of Minecraft Bedrock and Minecraft Java servers with a modern interface and optimized performance.</p>

<div align="center">

![Server App Desktop](https://img.shields.io/badge/Version-1.0.0.3%20(Preview)-darkblue?style=for-the-badge)
![.NET](https://img.shields.io/badge/.NET-10.0-purple?style=for-the-badge)
![WinUI](https://img.shields.io/badge/WinUI-3-blue?style=for-the-badge)
![Windows App SDK](https://img.shields.io/badge/Windows%20App%20SDK-1.8-green?style=for-the-badge)
![Platform](https://img.shields.io/badge/Platform-Windows%2010/11-lightgrey?style=for-the-badge)

</div>

---

## App Architecture

### Design
- Starting with the 1.0 Preview 2 update, Server App Desktop transitioned to WinUI 3/Windows App SDK, which is closely integrated with the Windows Fluent Design System (Fluent 2).
- Fluent 2 enables superior UI design and animations without compromising application performance.
- The entire codebase is being rebuilt using the **Model-View-ViewModel** (MVVM) pattern.
- Multi-language support is currently being implemented; we invite you to contribute by translating strings into your preferred language.

### Source Code
- Developed using the new **.NET 10.0 SDK** and **C# 14** to ensure compatibility with Windows 10 and 11.
- Utilizing Windows App SDK version 1.8, featuring support for unpackaged deployment (non-MSIX).
- Organized into separate projects (Executable-Library1-Library2-Library3) for better clarity and more efficient unit testing.
- Implementing Dependency Injection (DI) via the `Microsoft.Extensions.Hosting` package for streamlined service management.
- Variable usage has been optimized to reduce memory overhead.
- **Directory Structure:**
    ```text
    ServerAppDesktop
    |   ServerAppDesktop.sln            # Solution
    |
    +---installer                       
    |   \---Assets                      # Installer assets
    \---src
        +---ServerAppDesktop            # Main project, UI, and ViewModels
        +---ServerAppDesktop.Controls   # Custom controls
        +---ServerAppDesktop.Helpers    # Helper classes
        +---ServerAppDesktop.Models     # Data models
        +---ServerAppDesktop.Converters # Value converters
        \---ServerAppDesktop.Services   # Services (Core logic)
    ```

## Features

### Unified Interface
- Management support for both Minecraft Bedrock and Minecraft Java servers.
- Native execution of server files (`server.jar` or `bedrock_server.exe`) on your PC, even without an internet connection.
- Command console support (send commands like the classic `say Hello World!`).
- Real-time performance monitoring (CPU, RAM, Network, and Disk I/O).
- System information at a glance without needing to open additional windows.
- Comprehensive file management (Delete, Copy, Move, Rename, Backup, and Edit).
- Integrated feedback system for reporting improvements.
- Full compliance with Fluent Design guidelines.

### Configuration
- All settings are saved in a human-readable JSON pattern.
- Persistent configuration stored in `%LocalAppData%\Server App Desktop\Settings\Settings.json` (user-editable).
- Supports both Per-User and Per-Machine installations.

## Installation and Setup

### Prerequisites
- **OS**: Windows 10 Version 2004 (19041) or higher.
- **Hardware**: x64, x86, or ARM64.
- **Visual Studio**: 2022 version 17.8+ with the "WinUI Application Development" workload installed.

### Installing from Release
1. **Download the installer**: `ServerAppDesktop-Setup-1.0.0.3-Preview.exe`
2. **Run as Administrator** for a full installation **(OPTIONAL)**.
3. **Follow the setup wizard** on the first launch for initial configuration.

### Building from Source

```powershell
# 1. Clone the repository
git clone [https://github.com/ProfMinecraftDev/ServerAppDesktop.git](https://github.com/ProfMinecraftDev/ServerAppDesktop.git)
cd ServerAppDesktop

# 2. Restore dependencies
dotnet restore ServerAppDesktop.sln

# 3. Build in Debug mode
dotnet build ServerAppDesktop.sln -c Debug

# 4. Build for Release (with optimizations)
dotnet publish ServerAppDesktop.sln -c Release -r win-x64 --self-contained
```

---

## Available Languages
This documentation is also available in the following languages:

| Language | Status | Link |
| :--- | :---: | :--- |
| ![Español](https://img.shields.io/badge/Español-es--419-green) | **Complete** | [README.md](/README.md) |

> [!TIP]
> Want to help? If you speak another language, your contributions to translate the documentation are welcome!