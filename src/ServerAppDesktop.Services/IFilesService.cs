namespace ServerAppDesktop.Services;

public interface IFilesService
{
    public Task<List<ServerFile>> GetFilesAsync(string path);
    public Task CopyAsync(string[] sources, string destination, bool isMove);
    public Task DeleteAsync(string path, bool isFile);
    public Task RenameAsync(string oldPath, string newName, bool isFile);
    public Task CreateItemAsync(string path, string name, bool isFile);
    public DriveInfo? GetDriveInfo(string path);
    public string FormatSize(double bytes);
}
