using Microsoft.UI.Xaml.Controls;
using ServerAppDesktop.ViewModels;
using ServerAppDesktop.Services;
using ServerAppDesktop.Views.Pages;
using ServerAppDesktop.Helpers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ServerAppDesktop.Views;

public sealed partial class MainView : Page
{
	public MainViewModel ViewModel { get; } = App.GetRequiredService<MainViewModel>();

	public MainView()
	{
		InitializeComponent();
		var navigationService = App.GetRequiredService<INavigationService>();
		navigationService.SetFrame(contentFrame);
		navigationService.SetNavigationView(navView);
	}

	private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
	{
		var navigationService = App.GetRequiredService<INavigationService>();
		if (args.IsSettingsInvoked)
		{
			navigationService.Navigate<SettingsPage>();
			return;
		}
	}

	private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
	{
		var navigationService = App.GetRequiredService<INavigationService>();
		if (args.SelectedItemContainer is NavigationViewItem selectedItem)
		{
			switch (selectedItem.Tag)
			{
				case "Home":
					navigationService.Navigate<HomePage>();
					break;
				case "Performance":
					navigationService.Navigate<PerformancePage>();
					break;
				case "Terminal":
					navigationService.Navigate<TerminalPage>();
					break;
				case "Files":
					navigationService.Navigate<FilesPage>();
					break;
				case "WhatsNew":
					navigationService.Navigate<WhatsNewPage>();
					break;
				case "SystemInfo":
					navigationService.Navigate<SystemInfoPage>();
					break;
				case "About":
					navigationService.Navigate<AboutPage>();
					break;
				case "Settings":
					navigationService.Navigate<SettingsPage>();
					break;
			}
			ViewModel.CanGoBack = contentFrame.CanGoBack;
		}
	}
}
