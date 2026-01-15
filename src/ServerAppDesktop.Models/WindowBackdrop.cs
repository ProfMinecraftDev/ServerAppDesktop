using Microsoft.UI.Xaml.Media;

namespace ServerAppDesktop.Models
{
    public sealed class WindowBackdrop
    {
        public string Name { get; set; } = "";
        public SystemBackdrop Value { get; set; } = new MicaBackdrop();
        public int Index { get; set; } = 0;
    }
}
