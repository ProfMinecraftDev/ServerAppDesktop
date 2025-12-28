using System;
using Microsoft.UI.Xaml.Controls;

namespace ServerAppDesktop.Services
{
	public interface INavigationService
	{
		event Action<bool>? CanGoBackChanged;
		void SetFrame(Frame frame);
		void SetNavigationView(NavigationView navigationView);
		void Navigate<TPage>() where TPage : Page, new();
		void GoBack();
	}
}
