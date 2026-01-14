using Microsoft.UI.Xaml.Controls;

namespace ServerAppDesktop.Models
{
    public sealed class ServerFile
    {
        public IconElement? Icon { get; set; }
        public string Name { get; set; } = "";
        public string Size { get; set; } = "";
        public string ModifiedDate { get; set; } = "";
        public bool IsEditable { get; set; }
        public bool IsFile { get; set; }
    }
}
