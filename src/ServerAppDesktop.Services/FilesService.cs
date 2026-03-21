namespace ServerAppDesktop.Services;

public sealed class FilesService : IFilesService
{
    public async Task<List<ServerFile>> GetFilesAsync(string path)
    {
        var filesData = await Task.Run(() =>
        {
            var di = new DirectoryInfo(path);
            return di.GetFileSystemInfos()
                .Select(i => new
                {
                    i.Name,
                    FullName = i.FullName,
                    IsFile = i is FileInfo,
                    LastWrite = i.LastWriteTime.ToString("dd/MM/yyyy HH:mm"),
                    Length = i is FileInfo f ? f.Length : 0
                })
                .OrderBy(e => e.IsFile).ThenBy(e => e.Name)
                .ToList();
        });

        return filesData.Select(d => new ServerFile(
            d.Name,
            d.IsFile ? FormatSize(d.Length) : "--",
            d.LastWrite,
            d.FullName,
            d.IsFile,
            d.IsFile ? new SymbolIcon(Symbol.Document) : new SymbolIcon(Symbol.Folder)
        )).ToList();
    }

    public async Task CopyAsync(string[] sources, string destination, bool isMove)
    {
        await Task.Run(() =>
        {
            foreach (string s in sources)
            {
                string d = Path.Combine(destination, Path.GetFileName(s));
                if (s.Equals(d, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (isMove)
                {
                    if (File.Exists(s))
                    { if (File.Exists(d)) File.Delete(d); File.Move(s, d); }
                    else if (Directory.Exists(s))
                    { if (Directory.Exists(d)) Directory.Delete(d, true); Directory.Move(s, d); }
                }
                else
                {
                    if (File.Exists(s))
                        File.Copy(s, d, true);
                    else if (Directory.Exists(s))
                        CopyDirectory(s, d);
                }
            }
        });
    }

    public async Task DeleteAsync(string path, bool isFile)
    {
        await Task.Run(() => { if (isFile) File.Delete(path); else Directory.Delete(path, true); });
    }

    public async Task RenameAsync(string oldPath, string newName, bool isFile)
    {
        string? dir = Path.GetDirectoryName(oldPath);
        if (dir == null)
            return;
        string newPath = Path.Combine(dir, newName);
        await Task.Run(() => { if (isFile) File.Move(oldPath, newPath); else Directory.Move(oldPath, newPath); });
    }

    public async Task CreateItemAsync(string path, string name, bool isFile)
    {
        string p = Path.Combine(path, name);
        await Task.Run(() => { if (isFile) { using (File.Create(p)) { } } else Directory.CreateDirectory(p); });
    }

    public DriveInfo? GetDriveInfo(string path)
    {
        try
        {
            string? root = Path.GetPathRoot(path);
            if (string.IsNullOrEmpty(root))
                return null;
            var info = new DriveInfo(root);
            return info.IsReady ? info : null;
        }
        catch { return null; }
    }

    public string FormatSize(double b)
    {
        string[] u = ["B", "KB", "MB", "GB", "TB"];
        int i = 0;
        while (b >= 1024 && i < u.Length - 1)
        { b /= 1024.0; i++; }
        return $"{b:N2} {u[i]}";
    }

    private void CopyDirectory(string s, string d)
    {
        Directory.CreateDirectory(d);
        foreach (string f in Directory.GetFiles(s))
            File.Copy(f, Path.Combine(d, Path.GetFileName(f)), true);
        foreach (string sub in Directory.GetDirectories(s))
            CopyDirectory(sub, Path.Combine(d, Path.GetFileName(sub)));
    }
}
