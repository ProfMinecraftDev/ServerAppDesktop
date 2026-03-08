namespace ServerAppDesktop.Models;

public sealed class ServerFile
{
    public IconElement? Icon { get; set; }
    public string Name { get; set; } = "";
    public string Size { get; set; } = "";
    public string ModifiedDate { get; set; } = "";
    public string AbsolutePath { get; set; } = string.Empty;
    public bool IsFile { get; set; }

    public ServerFile(string name, string size, string modifiedDate, string absolutePath, bool isFile = true, IconElement? icon = null)
    {
        Name = name;
        Size = size;
        ModifiedDate = modifiedDate;
        AbsolutePath = absolutePath;
        IsFile = isFile;
        Icon = icon;
    }
}
