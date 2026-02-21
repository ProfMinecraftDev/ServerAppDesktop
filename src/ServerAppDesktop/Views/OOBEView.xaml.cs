namespace ServerAppDesktop.Views
{
    public sealed partial class OOBEView : Page
    {
        public OOBEViewModel ViewModel { get; }

        public OOBEView()
        {
            InitializeComponent();
            ViewModel = App.GetRequiredService<OOBEViewModel>();
        }
    }
}
