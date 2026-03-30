namespace ServerAppDesktop.Views.Pages;

public sealed partial class FilesPage : Page
{
    public FilesViewModel ViewModel { get; }
    private readonly SemaphoreSlim _dialogSemaphore = new(1, 1);

    public FilesPage()
    {
        InitializeComponent();
        ViewModel = App.GetRequiredService<FilesViewModel>();

        ViewModel.ErrorOccurred += async (s, e) =>
        {
            DispatcherQueue.TryEnqueue(async () =>
            {
                detailsExpander.IsExpanded = false;
                await ShowError(e.Message, e.Details);
                detailsExpander.IsExpanded = true;
            });
        };

        ViewModel.OperationSuccess += async (s, e) =>
        {
            DispatcherQueue.TryEnqueue(async () =>
            {
                successDialog.Content = e.Message;
                await successDialog.ShowAsync();
                successDialog.Content = null;
            });
        };
    }

    private void ListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (ViewModel.SetPathCommand.CanExecute(null))
            ViewModel.SetPathCommand.Execute(null);
    }

    private async void BackupButton_Click(object sender, RoutedEventArgs e)
    {
        await ShowDialogSafe(backupDialog);
    }

    private async void RenameCommand_Requested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        if (ViewModel.SelectedServerFile == null)
            return;

        newNameTextBox.Text = ViewModel.SelectedServerFile.Name;
        await ShowDialogSafe(renameDialog);
    }

    private async void DeleteCommand_Requested(XamlUICommand sender, ExecuteRequestedEventArgs args)
    {
        if (ViewModel.SelectedServerFile == null)
            return;
        await ShowDialogSafe(deleteDialog);
    }

    private async Task ShowDialogSafe(ContentDialog dialog)
    {
        if (_dialogSemaphore.CurrentCount == 0)
            return;

        await _dialogSemaphore.WaitAsync();
        try
        {
            await dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            string errorLog = string.Format(ResourceHelper.GetString("FilesPage_DialogError"), ex.Message);
            System.Diagnostics.Debug.WriteLine(errorLog);
        }
        finally
        {
            _dialogSemaphore.Release();
        }
    }

    private async Task ShowError(string message, string details = "")
    {
        errorMessageText.Text = message;
        errorDetailsText.Text = string.IsNullOrEmpty(details)
            ? ResourceHelper.GetString("FilesPage_NoDetails")
            : details;

        try
        {
            await errorDialog.ShowAsync();
        }
        catch (Exception ex)
        {
            string errorLog = string.Format(ResourceHelper.GetString("FilesPage_DialogError"), ex.Message);
            Debug.WriteLine(errorLog);
        }
    }

    private async void CreateFolder_Click(object sender, RoutedEventArgs e)
    {
        await ShowDialogSafe(createFolderDialog);
    }

    private async void CreateFile_Click(object sender, RoutedEventArgs e)
    {
        await ShowDialogSafe(createFileDialog);
    }

    private void DataRow_RightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        DependencyObject container = sender.To<UIElement>();
        while (container is not ListViewItem && container != null)
        {
            container = VisualTreeHelper.GetParent(container);
        }

        if (container is ListViewItem item)
        {
            item.IsSelected = true;
            e.Handled = true;
        }
    }
}
