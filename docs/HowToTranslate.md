<h1 align="center">
    Plan de Traducción para Server App Desktop
</h1>

<p align="center">
  <img src="images/Translate.png" width="800">
</p>

## Análisis del Sistema de Traducción

Server App Desktop utiliza **WinUI 3** con **archivos RESW** (Resources) para la localización. Este es el sistema estándar de Microsoft para aplicaciones modernas de Windows.

### Estructura de Archivos de Traducción

```
src/ServerAppDesktop/Strings/
├── es-419/
│   └── Resources.resw     ← Plantilla maestra en español (Latinoamérica)
└── en-US/
    └── Resources.resw     ← Traducción al inglés (Estados Unidos)
```

**¡Buenas noticias!** La traducción maestra al **español (es-419)** ya está **100% completa** con más de **400 cadenas traducidas** de alta calidad.

### Cómo Funciona el Sistema

1. **ResourceLoader** (en `ResourceHelper.cs`) carga automáticamente los strings según el idioma seleccionado:
   ```csharp
   private static readonly ResourceLoader _resourceLoader = new();
   public static string GetString(string resourceKey) => _resourceLoader.GetString(resourceKey);
   ```

2. **Clave → Valor**: Cada string tiene un `name` único que se referencia en XAML/C#:
   ```xml
   <data name="StartServerText.Text" xml:space="preserve">
       <value>Iniciar</value>  <!-- Traducción es-419 -->
   </data>
   ```

3. **Idiomas disponibles**: 
- `es-419` (Español - Latinoamérica) ✅ **Maestra**
- `en-US` (English - United States) ✅ **Completa**

## Guía Paso a Paso para Crear Nueva Traducción

### 1. **Preparar Nueva Carpeta de Idioma**
```
src/ServerAppDesktop/Strings/[NUEVO_CODIGO]/
└── Resources.resw
```
**Códigos comunes**:
- `pt-BR` (Português Brasileiro)
- `fr-FR` (Français)
- `de-DE` (Deutsch)
- `it-IT` (Italiano)
> Debes usar código `BCP-47` (es decir, idioma-pais).

### 2. **Copiar Plantilla desde en-US**
```bash
# En PowerShell o CMD
copy "src\\ServerAppDesktop\\Strings\\es-419\\Resources.resw" "src\\ServerAppDesktop\\Strings\\pt-BR\\Resources.resw"
```

### 3. **Traducir Cada `<data>`**
**Ejemplo**:
```xml
<!-- ANTES (en-US) -->
<data name="StartServerText.Text" xml:space="preserve">
    <value>Start</value>
</data>

<!-- DESPUÉS (pt-BR) -->
<data name="StartServerText.Text" xml:space="preserve">
    <value>Iniciar</value>
</data>
```

**✅ Mantén**:
- `name` **exactamente igual**
- Estructura XML
- `xml:space="preserve"`

**❌ Cambia**:
- Solo el contenido entre `<value></value>`

**⚠️ Advertencia**:
- Toma en cuenta las strings formateable (aquellas con `{0}` o `{1}`)

### 4. **Herramientas Recomendadas**
- **VS Code** con extensión **ResX Manager** ✅ **Gratis**
- **Visual Studio** → Herramientas → ResxManager
- **Notepad++** para ediciones rápidas

### 5. **Agregar Traducción**
En `SettingsService.cs` en el método `GetLanguageIndex()`
```csharp
    public int GetLanguageIndex()
    {
        string code = DataHelper.Settings?.UI.Language?.ToLowerInvariant() ?? "";
        return code switch
        {
            "" => 0,
            "es-419" => 0,
            "en-us" => 1,
            "pt-br" => 3, // El nuevo idioma
            _ => 0
        };
    }
```

### 6. **Probar Traducción**
```powershell
# Cambiar idioma temporalmente en Settings.json
# %LocalAppData%\\Server App Desktop\\Settings\\Settings.json
"Language": "pt-BR"
```
Reinicia la app → Verifica todos los strings.

### 7. **Contribuir con Pull Request**
1. **Commit** cambios
2. **Push** a tu fork
3. **Abrir PR** → `features/translation-[idioma]`

## Estructura de Cadenas Principales

| Categoría | Ejemplos | Cantidad |
|-----------|----------|----------|
| **UI Botones** | Start, Stop, Settings | ~50 |
| **Diálogos** | Welcome, Error, Success | ~80 |
| **Navegación** | Home, Files, Terminal | ~20 |
| **Configuración** | RAM Limit, Difficulty | ~100 |
| **Archivos** | New File, Delete, Backup | ~60 |
| **Notificaciones** | Server Started, Error | ~40 |

**Total: 400+ strings**

## Ejemplo Completo: Agregar Portugués

```xml
<!-- src/ServerAppDesktop/Strings/pt-BR/Resources.resw -->
<data name="OOBEView_WelcomeText.Text" xml:space="preserve">
    <value>Bem-vindo ao Server App Desktop</value>
</data>
<data name="StartServerText.Text" xml:space="preserve">
    <value>Iniciar</value>
</data>
<!-- ... Traducir todos los <data> ... -->
```

## Traducción de Documentación

Para traducir la documentación al nuevo idioma:

### 1. Crear estructura
```
docs/[NUEVO_CODIGO]/
├── HowToTranslate.md
└── README.md
```

### 2. Copiar plantillas
```bash
copy "docs\\HowToTranslate.md" "docs\\[NUEVO_CODIGO]\\HowToTranslate.md"
copy "docs\\en-US\\README.md" "docs\\[NUEVO_CODIGO]\\README.md"
```

### 3. Traducir contenido Markdown
Mantén estructura, tablas, código y enlaces. Traduce solo texto narrativo.

## Soporte Adicional

- **Cambiar idioma runtime**: Settings → Language
- **Fallback**: Si falta string → Muestra `es-419` (idioma maestro)
- **Documentación en inglés**: [docs/en-US/HowToTranslate.md](/docs/en-US/HowToTranslate.md)

---

**¡Gracias por ayudar a globalizar Server App Desktop!** 🌍

**Estado actual**:  
✅ **es-419 Maestra** \| ✅ **en-US Completa** \| 🔄 **Otros idiomas: ¡Necesitamos tu ayuda!**

---

**¿Listo para traducir?** Copia `en-US/Resources.resw`, traduce y abre un PR. ¡Tu contribución llegará a miles de usuarios! 🚀
