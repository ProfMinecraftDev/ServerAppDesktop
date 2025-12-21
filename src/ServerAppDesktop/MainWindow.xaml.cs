using ServerAppDesktop.Helpers;
using WinUIEx;

namespace ServerAppDesktop
{
    public sealed partial class MainWindow : WindowEx
    {
        public string WindowTitle => DataHelper.WindowTitle;
        public string WindowSubtitle => DataHelper.WindowSubtitle;

        public MainWindow()
        {
            InitializeComponent();
            ExtendsContentIntoTitleBar = true;
        }
    }
}
