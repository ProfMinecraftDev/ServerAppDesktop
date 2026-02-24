namespace ServerAppDesktop.Handlers;

public interface IWindowHandler
{
    public bool WindowClosed { get; }
    public void SetWindow(MainWindow window);
    public void Configure();
    public void HandleNetworkUIUpdate(InfoBar infoBar, bool isConnected);
    public Task UpdateFullScreenLogic(bool fullScreen, Button fsButton);
    public void SetBadgeIcon(string iconPath);
}
