
namespace ServerAppDesktop
{
    public sealed partial class AppTrayIcon : TaskbarIcon
    {
        private readonly TrayViewModel _viewModel = App.GetRequiredService<TrayViewModel>();
        private readonly HomeViewModel _homeViewModel = App.GetRequiredService<HomeViewModel>();

        public TrayViewModel ViewModel => _viewModel;
        public HomeViewModel HomeViewModel => _homeViewModel;


        public AppTrayIcon()
        {
            InitializeComponent();
        }
    }
}
