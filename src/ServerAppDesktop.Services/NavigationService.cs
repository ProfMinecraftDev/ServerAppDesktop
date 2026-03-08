
namespace ServerAppDesktop.Services;

public class NavigationService : INavigationService
{
    private Frame? _frame;
    private NavigationView? _navigationView;
    public event TypedEventHandler<INavigationService, CanGoBackChangedEventArgs>? CanGoBackChanged;

    public void SetFrame(Frame frame)
    {
        _frame = frame;
        _frame.Navigated += OnNavigated;
    }

    public void SetNavigationView(NavigationView navigationView)
    {
        _navigationView = navigationView;
    }

    public void Navigate<TPage>() where TPage : Page, new()
    {
        _ = (_frame?.Navigate(typeof(TPage), null, new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight }));
    }

    public void GoBack()
    {
        if (CanGoBack)
        {
            _frame?.GoBack(new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromLeft });
        }
    }

    private bool CanGoBack => _frame?.CanGoBack == true;

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        if (_navigationView == null || _frame == null)
        {
            return;
        }
        else
        {
            string pageName = e.SourcePageType.Name.Replace("Page", "");

            _navigationView.SelectedItem = _navigationView.MenuItems
                .Concat(_navigationView.FooterMenuItems)
                .OfType<NavigationViewItem>()
                .FirstOrDefault(item => (item.Tag as string) == pageName);
        }

        CanGoBackChanged?.Invoke(this, new CanGoBackChangedEventArgs(CanGoBack));
    }

}
