# Translation Plan for Server App Desktop

## Translation System Analysis

Server App Desktop uses **WinUI 3** with **RESW** (Resources) files for localization. This is Microsoft's standard system for modern Windows applications.

### Translation Files Structure

```
src/ServerAppDesktop/Strings/
├── es-419/
│   └── Resources.resw     ← Master template in Spanish (Latin America)
└── en-US/
    └── Resources.resw     ← English (United States) translation
```

**Good news!** The master translation to **Spanish (es-419)** is **100% complete** with over **400 high-quality translated strings**.

### How the System Works

1. **ResourceLoader** (in `ResourceHelper.cs`) automatically loads strings based on the selected language:
   ```csharp
   private static readonly ResourceLoader _resourceLoader = new();
   public static string GetString(string resourceKey) => _resourceLoader.GetString(resourceKey);
   ```

2. **Key → Value**: Each string has a unique `name` referenced in XAML/C#:
   ```xml
   <data name="StartServerText.Text" xml:space="preserve">
       <value>Start</value>  <!-- es-419 translation -->
   </data>
   ```

3. **Available languages**: 
- `es-419` (Spanish - Latin America) ✅ **Master**
- `en-US` (English - United States) ✅ **Complete**

## Step-by-Step Guide to Create New Translation

### 1. **Prepare New Language Folder**
```
src/ServerAppDesktop/Strings/[NEW_CODE]/
└── Resources.resw
```
**Common codes**:
- `pt-BR` (Brazilian Portuguese)
- `fr-FR` (French)
- `de-DE` (German)
- `it-IT` (Italian)
> Use `BCP-47` code (language-country).

### 2. **Copy Template from Master (es-419)**
```bash
# In PowerShell or CMD
copy "src\\ServerAppDesktop\\Strings\\es-419\\Resources.resw" "src\\ServerAppDesktop\\Strings\\pt-BR\\Resources.resw"
```

### 3. **Translate Each `<data>`**
**Example**:
```xml
<!-- BEFORE (Master es-419) -->
<data name="StartServerText.Text" xml:space="preserve">
    <value>Iniciar</value>
</data>

<!-- AFTER (pt-BR) -->
<data name="StartServerText.Text" xml:space="preserve">
    <value>Iniciar</value>
</data>
```

**✅ Keep**:
- `name` **exactly the same**
- XML structure
- `xml:space="preserve"`

**❌ Change**:
- Only content between `<value></value>`

**⚠️ Warning**:
- Consider formatted strings (those with `{0}` or `{1}`)

### 4. **Recommended Tools**
- **VS Code** with **ResX Manager** extension ✅ **Free**
- **Visual Studio** → Tools → ResxManager
- **Notepad++** for quick edits

### 5. **Add Translation**
In `SettingsService.cs` in the `GetLanguageIndex()` method
```csharp
    public int GetLanguageIndex()
    {
        string code = DataHelper.Settings?.UI.Language?.ToLowerInvariant() ?? "";
        return code switch
        {
            "" => 0,
            "es-419" => 0,
            "en-us" => 1,
            "pt-br" => 3, // The new language
            _ => 0
        };
    }
```

### 6. **Test Translation**
```powershell
# Temporarily change language in Settings.json
# %LocalAppData%\\Server App Desktop\\Settings\\Settings.json
"Language": "pt-BR"
```
Restart the app → Verify all strings.

### 7. **Contribute with Pull Request**
1. **Commit** changes
2. **Push** to your fork
3. **Open PR** → `features/translation-[language]`

## Main Strings Structure

| Category | Examples | Quantity |
|-----------|----------|----------|
| **UI Buttons** | Start, Stop, Settings | ~50 |
| **Dialogs** | Welcome, Error, Success | ~80 |
| **Navigation** | Home, Files, Terminal | ~20 |
| **Configuration** | RAM Limit, Difficulty | ~100 |
| **Files** | New File, Delete, Backup | ~60 |
| **Notifications** | Server Started, Error | ~40 |

**Total: 400+ strings**

## Complete Example: Add Portuguese

```xml
<!-- src/ServerAppDesktop/Strings/pt-BR/Resources.resw -->
<data name="OOBEView_WelcomeText.Text" xml:space="preserve">
    <value>Welcome to Server App Desktop</value>
</data>
<data name="StartServerText.Text" xml:space="preserve">
    <value>Start</value>
</data>
<!-- ... Translate all <data> ... -->
```

## Documentation Translation

To translate documentation to the new language:

### 1. Create structure
```
docs/[NEW_CODE]/
├── HowToTranslate.md
└── README.md
```

### 2. Copy templates
```bash
copy "docs\\HowToTranslate.md" "docs\\[NEW_CODE]\\HowToTranslate.md"
copy "README.md" "docs\\[NEW_CODE]\\README.md"
```

### 3. Translate Markdown content
Keep structure, tables, code, and links. Translate only narrative text.

## Additional Support

- **Change language runtime**: Settings → Language
- **Fallback**: If string missing → Shows `es-419` (master language)
- **Spanish documentation**: [docs/HowToTranslate.md](docs/HowToTranslate.md)

---

**Thank you for helping globalize Server App Desktop!** 🌍

**Current status**:  
✅ **es-419 Master** \| ✅ **en-US Complete** \| 🔄 **Other languages: We need your help!**

---

**Ready to translate?** Copy `es-419/Resources.resw`, translate and open a PR. Your contribution will reach thousands of users! 🚀
