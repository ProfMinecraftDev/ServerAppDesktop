


namespace ServerAppDesktop.Views.Pages;




public sealed partial class AboutPage : Page
{
    public AboutPage()
    {
        InitializeComponent();
    }

    private void SelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        if (sender.SelectedItem is SelectorBarItem item && item != null)
        {
            Type? pageType = item.Tag?.ToString() switch
            {
                "AboutApp" => typeof(AppInfoPage),
                "AboutLicenses" => typeof(ThirdPartyLicensesPage),
                _ => null
            };

            if (pageType != null)
            {
                _ = aboutFrame.Navigate(pageType, null, new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight });
            }
        }
    }
}
