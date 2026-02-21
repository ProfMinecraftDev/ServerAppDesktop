namespace ServerAppDesktop.Handlers;

public interface IWindowHandler
{
    bool WindowClosed { get; }
    void SetWindow(MainWindow window);
    void Configure();
    void HandleNetworkUIUpdate(InfoBar infoBar, bool isConnected);
    Task UpdateFullScreenLogic(bool fullScreen, Button fsButton);
    void SetIcon(string iconPath);
}
