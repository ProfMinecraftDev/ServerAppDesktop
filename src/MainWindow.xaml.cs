using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using System.ComponentModel;
using Windows.Graphics;
using Microsoft.UI.Text;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ServerAppDesktop
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window, INotifyPropertyChanged
    {
        public Grid M_Layout => MainLayout;
        public Frame M_Frame => MainFrame;

        public new string Title
        {
            get => base.Title;
            set
            {
                if (base.Title != value)
                {
                    base.Title = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindow()
        {
            this.InitializeComponent();
            this.ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(AppTitleBar);

            var hwnd = WindowNative.GetWindowHandle(this);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);
            appWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;

#if DEBUG
            var headgrid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Background = (Brush)Application.Current.Resources["SystemControlHighlightAccentBrush"],
            };
            var head = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            var ring = new ProgressRing
            {
                IsActive = true,
                Width = 18,
                Height = 18,
                FontWeight = FontWeights.Bold,
                IsIndeterminate = true,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = (Brush)Application.Current.Resources["SystemControlForegroundBaseHighBrush"],
                HorizontalAlignment = HorizontalAlignment.Center
            };
            head.Children.Add(ring);
            var textBlock = new TextBlock
            {
                Text = "Debug Mode",
                FontWeight = FontWeights.Bold,
                FontFamily = new FontFamily("Consolas"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            head.Children.Add(textBlock);
            AppTitleBar.Children.Add(headgrid);
            headgrid.Children.Add(head);
            AppTitleBar.Margin = new Thickness(12, 0, 0, 0);
            Grid.SetRow(headgrid, 0);
#endif
        }

    }
}



