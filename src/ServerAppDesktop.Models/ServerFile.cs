namespace ServerAppDesktop.Models;

public sealed class ServerFile(string name, string size, string modifiedDate, string absolutePath, bool isFile = true, IconElement? icon = null)
{
    public IconElement? Icon { get; set; } = icon;
    public string Name { get; set; } = name;
    public string Size { get; set; } = size;
    public string ModifiedDate { get; set; } = modifiedDate;
    public string AbsolutePath { get; set; } = absolutePath;
    public bool IsFile { get; set; } = isFile;
}
