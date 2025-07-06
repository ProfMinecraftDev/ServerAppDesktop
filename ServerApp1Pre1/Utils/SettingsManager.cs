// Archivo: Utils/SettingsManager.cs
// Este archivo contiene la lógica para cargar y guardar la configuración de la aplicación en un archivo JSON
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace ServerApp1Pre1.Utils
{
    // Clase para manejar la configuración de la aplicación
    public static class SettingsManager
    {
        private const string FileName = "settings.json"; // Nombre del archivo de configuración
        
        // Obtiene la ruta de la carpeta de datos de la aplicación
        private static string GetAppDataPath()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(appDataPath, "Server App Desktop");
            
            // Crear la carpeta si no existe
            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }
            
            return appFolder;
        }
        
        // Obtiene la ruta completa del archivo de configuración
        private static string GetSettingsFilePath()
        {
            return Path.Combine(GetAppDataPath(), FileName);
        }

        // Clase que representa la configuración de la aplicación
        public static async Task<AppSettings> LoadAsync()
        {
            var filePath = GetSettingsFilePath();
            try
            {
                if (!File.Exists(filePath))
                    return new AppSettings(); // Si no existe, devuelve configuración por defecto

                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                return await JsonSerializer.DeserializeAsync<AppSettings>(stream) ?? new AppSettings(); // Deserializa el contenido del archivo a un objeto AppSettings
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings: {ex.Message}"); // Maneja cualquier excepción que ocurra al cargar la configuración
                return new AppSettings(); // Devuelve configuración por defecto en caso de error
            }
        }

        // Método para guardar la configuración de la aplicación en un archivo JSON
        // Recibe un objeto AppSettings y lo serializa a JSON, guardándolo en el archivo de configuración
        // Si el archivo ya existe, lo reemplaza
        public static async Task SaveAsync(AppSettings settings) 
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings)); // Verifica que los ajustes no sean nulos
            
            var filePath = GetSettingsFilePath();
            using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            await JsonSerializer.SerializeAsync(stream, settings, new JsonSerializerOptions { WriteIndented = true }); // Serializa el objeto AppSettings a JSON y lo escribe en el archivo
        }
    }
}
