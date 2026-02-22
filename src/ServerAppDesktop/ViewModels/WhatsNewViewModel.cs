namespace ServerAppDesktop.ViewModels;

public sealed partial class WhatsNewViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isLoaded;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isDisconnected;

    [ObservableProperty]
    private string _content = string.Empty;

    public WhatsNewViewModel()
    {
        _ = LoadContentAsync();
    }

    [RelayCommand]
    private async Task ReloadContentAsync()
    {
        await LoadContentAsync();
    }
    private async Task LoadContentAsync()
    {
        IsLoading = true;
        IsLoaded = false;
        IsDisconnected = false;

        try
        {
            Content = await UpdateHelper.GetNewsOfLatestRelease(DataHelper.GitHubUsername, DataHelper.GitHubRepository, DataHelper.UpdateChannel == 1);

            if (string.IsNullOrEmpty(Content))
            {
                IsDisconnected = true;
            }
            else
            {
                IsLoaded = true;
            }
        }
        catch (Exception)
        {
            IsDisconnected = true;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
