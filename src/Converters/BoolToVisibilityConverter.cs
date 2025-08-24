using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace ServerAppDesktop.Converters
{
    /// <summary>
    /// Converter que convierte un bool a Visibility (Visible/Collapsed)
    /// </summary>
    public sealed class BoolToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Si es true, invierte el resultado (true = Collapsed, false = Visible)
        /// </summary>
        public bool IsInverted { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                bool boolValue = false;
                
                if (value is bool directBool)
                {
                    boolValue = directBool;
                }
                else if (value != null && bool.TryParse(value.ToString(), out bool parsedBool))
                {
                    boolValue = parsedBool;
                }

                // Aplicar inversión si es necesaria
                if (IsInverted)
                    boolValue = !boolValue;

                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en BoolToVisibilityConverter: {ex.Message}");
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            try
            {
                if (value is Visibility visibility)
                {
                    bool result = visibility == Visibility.Visible;
                    
                    // Aplicar inversión si es necesaria
                    if (IsInverted)
                        result = !result;
                        
                    return result;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en BoolToVisibilityConverter ConvertBack: {ex.Message}");
            }

            return false;
        }
    }
}
