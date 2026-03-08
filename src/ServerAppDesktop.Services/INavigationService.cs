namespace ServerAppDesktop.Services;

public interface INavigationService
{
    public event TypedEventHandler<INavigationService, CanGoBackChangedEventArgs>? CanGoBackChanged;
    public void SetFrame(Frame frame);
    public void SetNavigationView(NavigationView navigationView);
    public void Navigate<TPage>() where TPage : Page, new();
    public void GoBack();
}
