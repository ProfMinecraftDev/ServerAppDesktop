# 📋 Guía para Contribuir - Server App Desktop

## 🎯 ¡Gracias por querer contribuir!
Apreciamos tu interés en mejorar Server App Desktop. Sigue estas pautas para que tu contribución sea aceptada.

## 🚀 Antes de Empezar

### 1. 📋 Reportar Bugs
- Busca primero en [Issues](https://github.com/ProfMinecraftDev/ServerAppDesktop/issues) si el bug ya fue reportado
- Usa la plantilla de bug report
- Incluye: **Windows version, .NET version, pasos para reproducir, logs**

### 2. 💡 Solicitar Features
- Describe claramente la feature
- Explica el **porqué** sería útil
- Incluye ejemplos de uso

## 🛠️ Proceso de Contribución

### 1. 🍴 Haz Fork del Repositorio
```bash
git clone https://github.com/ProfMinecraftDev/ServerAppDesktop.git
cd ServerAppDesktop
```

### 2. 🌿 Crea una Rama
```bash
git checkout -b feature/tu-feature
# o
git checkout -b fix/tu-fix
```

### 3. 💻 Desarrolla tu Contribución

#### Estructura del Proyecto (ejemplo):
```
src/
├── Bedrock/          # Páginas principales
├── Services/         # Lógica de negocio  
├── Models/           # Modelos de datos
├── Converters/       # Conversores XAML
└── Utils/            # Utilidades
```

#### Convenciones de Código:
- **C#**: Sigue las [convenciones de .NET](https://docs.microsoft.com/dotnet/csharp/fundamentals/coding-style)
- **XAML**: Usa `x:Name` en camelCase, recursos ordenados
- **Comentarios**: Documenta código complejo con `///`

### 4. ✅ Prueba tu Código
```bash
# Ejecuta pruebas
dotnet test

# Prueba en diferentes Windows
# - Windows 10 2004+
# - Windows 11
# - Diferentes arquitecturas (x64, ARM64)
```

### 5. 📝 Haz Commit
```bash
git add .
git commit -m "Add: Descripción clara de los cambios"
```

### 6. 📤 Haz Push y Abre un PR
```bash
git push origin feature/tu-feature
```

## 🎨 Guidelines Técnicas

### Para Código C#:
```csharp
// ✅ Bien
public class ServerService
{
    private readonly ILogger _logger;
    
    public async Task<bool> StartServerAsync()
    {
        // Lógica aquí
    }
}

// ❌ Evitar
public class server_service
{
    public bool start_server()
    {
        // Lógica aquí
    }
}
```

### Para XAML:
```xml
<!-- ✅ Bien -->
<Button x:Name="installButton"
        Content="Instalar"
        Click="InstallButton_Click"/>

<!-- ❌ Evitar -->
<Button x:Name="Install_Button"
        Content="Instalar"
        Click="install_button_click"/>
```

## 📋 Plantilla de Pull Request

```markdown
## 📖 Descripción
[Describe tus cambios claramente]

## 🎯 Tipo de Cambio
- [ ] 🐛 Bug fix
- [ ] ✨ Nueva feature
- [ ] ♻️ Refactor
- [ ] 📚 Documentación
- [ ] 🧪 Tests

## ✅ Checklist
- [ ] Probado en Windows 10
- [ ] Probado en Windows 11  
- [ ] No rompe compatibilidad
- [ ] Documentación actualizada
- [ ] Sigue las convenciones de código

## 📸 Capturas (si aplica)
[Agrega capturas de pantalla si es un cambio visual]

## 🔍 Issues Relacionados
Fixes # [número de issue]
```

## 🧪 Testing

### Pruebas Requeridas:
- [ ] Funciona en Windows 10 2004+
- [ ] Funciona en Windows 11
- [ ] Instalación limpia funciona
- [ ] Actualización desde versión anterior funciona
- [ ] No hay regresiones

### Entornos de Prueba:
```bash
# Windows 10 - Versión mínima
OS: Windows 10 2004 (19041)
.NET: 10.0

# Windows 11 - Latest
OS: Windows 11 25H2
.NET: 10.0
```

## 📚 Documentación

### Actualiza documentación si:
- [ ] Agregas nueva feature
- [ ] Cambias comportamiento existente
- [ ] Modificas API pública
- [ ] Cambias proceso de instalación

## 🏆 Reconocimiento

Todas las contribuciones válidas serán:
- ✅ Reconocidas en el README.md
- ✅ Mencionadas en release notes
- ✅ Acreditadas como colaboradores

## ❌ Razones comunes para rechazar PRs

- No sigue convenciones de código
- Rompe compatibilidad
- Sin pruebas adecuadas
- Documentación incompleta
- Cambios demasiado amplios sin discusión previa

## 📞 ¿Necesitas ayuda?

- 📧 **Email**: ProfMinecraftDev@gmail.com
- 🐛 **Issues**: Abre un issue en GitHub
- 💬 **Discusiones**: Usa GitHub Discussions

---

**¡Gracias por hacer Server App Desktop mejor!** 🚀