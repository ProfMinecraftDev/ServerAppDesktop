using System;
using System.IO;
using System.Linq;

namespace ServerAppDesktop.Models
{
    public enum FileItemType
    {
        File,
        Directory,
        ConfigFile,
        LogFile,
        WorldFile,
        BackupFile
    }

    public class ServerFileItem
    {
        public string Name { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public FileItemType Type { get; set; }
        public long Size { get; set; }
        public DateTime LastModified { get; set; }
        public string DisplaySize { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public bool IsEditable { get; set; }
        public bool IsImportant { get; set; }

        public ServerFileItem()
        {
        }

        public ServerFileItem(string fullPath)
        {
            DisplaySize = string.Empty;
            Icon = string.Empty;
            FullPath = fullPath;
            Name = Path.GetFileName(fullPath);
            
            if (Directory.Exists(fullPath))
            {
                Type = FileItemType.Directory;
                Icon = "📁";
                Size = 0;
                DisplaySize = "-";
                IsEditable = false;
                
                try
                {
                    LastModified = Directory.GetLastWriteTime(fullPath);
                }
                catch
                {
                    LastModified = DateTime.MinValue;
                }
            }
            else if (File.Exists(fullPath))
            {
                try
                {
                    var fileInfo = new FileInfo(fullPath);
                    Size = fileInfo.Length;
                    LastModified = fileInfo.LastWriteTime;
                    DisplaySize = FormatFileSize(Size);
                    
                    // Determinar tipo de archivo y propiedades
                    var extension = Path.GetExtension(fullPath).ToLowerInvariant();
                    var fileName = Name.ToLowerInvariant();
                    
                    if (fileName == "server.properties" || fileName == "permissions.json" || fileName == "allowlist.json" || fileName == "whitelist.json")
                    {
                        Type = FileItemType.ConfigFile;
                        Icon = "⚙️";
                        IsEditable = true;
                        IsImportant = true;
                    }
                    else if (extension == ".log" || fileName.Contains("log"))
                    {
                        Type = FileItemType.LogFile;
                        Icon = "📄";
                        IsEditable = false;
                        IsImportant = true;
                    }
                    else if (extension == ".mcworld" || extension == ".mctemplate" || fileName.Contains("world"))
                    {
                        Type = FileItemType.WorldFile;
                        Icon = "🌍";
                        IsEditable = false;
                        IsImportant = true;
                    }
                    else if (extension == ".zip" || extension == ".tar" || extension == ".gz" || fileName.Contains("backup"))
                    {
                        Type = FileItemType.BackupFile;
                        Icon = "📦";
                        IsEditable = false;
                        IsImportant = false;
                    }
                    else
                    {
                        Type = FileItemType.File;
                        Icon = GetFileIcon(extension);
                        IsEditable = IsTextFile(extension);
                        IsImportant = false;
                    }
                }
                catch
                {
                    Size = 0;
                    LastModified = DateTime.MinValue;
                    DisplaySize = "Error";
                    Type = FileItemType.File;
                    Icon = "📄";
                    IsEditable = false;
                    IsImportant = false;
                }
            }
        }

        private static string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;
            
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            
            return string.Format("{0:n1} {1}", number, suffixes[counter]);
        }

        private static string GetFileIcon(string extension)
        {
            return extension switch
            {
                ".txt" or ".md" or ".readme" => "📝",
                ".json" => "🔧",
                ".properties" => "⚙️",
                ".xml" => "📋",
                ".jar" => "☕",
                ".exe" => "⚡",
                ".zip" or ".rar" or ".7z" => "📦",
                ".png" or ".jpg" or ".jpeg" or ".gif" => "🖼️",
                ".mp3" or ".wav" or ".ogg" => "🔊",
                ".cfg" or ".conf" or ".ini" => "🔧",
                _ => "📄"
            };
        }

        private static bool IsTextFile(string extension)
        {
            string[] textExtensions = {
                ".txt", ".md", ".json", ".properties", ".xml", ".cfg", 
                ".conf", ".ini", ".yml", ".yaml", ".log", ".csv"
            };
            
            return textExtensions.Contains(extension.ToLowerInvariant());
        }
    }
}
