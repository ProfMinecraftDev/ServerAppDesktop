using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Data;
using System;
using Windows.UI.Text;

namespace ServerAppDesktop.Converters
{
    /// <summary>
    /// Converter que transforma un valor booleano en FontWeight (Bold/Normal).
    /// </summary>
    public sealed class BoolToFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                if (value is bool boolValue)
                {
                    return boolValue ? FontWeights.Bold : FontWeights.Normal;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en Convert: {ex.Message}");
            }

            // Valor por defecto si el binding falla
            return FontWeights.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            try
            {
                if (value is FontWeight fontWeight)
                {
                    // Compara contra el peso de FontWeights.Normal
                    return fontWeight.Weight > FontWeights.Normal.Weight;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en ConvertBack: {ex.Message}");
            }

            // Valor por defecto si el binding falla
            return false;
        }
    }
}