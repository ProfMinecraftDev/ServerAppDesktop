




namespace ServerAppDesktop.Views.Pages
{



    public sealed partial class TerminalPage : Page
    {
        private readonly List<string> _commandHistory = [];
        private int _historyIndex = -1;
        public TerminalViewModel ViewModel => App.GetRequiredService<TerminalViewModel>();
        public TerminalPage()
        {
            InitializeComponent();
        }

        private void CommandInput_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            TextBox? textBox = sender as TextBox;
            if (textBox == null)
            {
                return;
            }

            switch (e.Key)
            {
                case VirtualKey.Enter:
                    string command = textBox.Text.Trim();
                    if (!string.IsNullOrWhiteSpace(command))
                    {

                        ViewModel.SendInputCommand.Execute(command);


                        if (_commandHistory.Count == 0 || _commandHistory[^1] != command)
                        {
                            _commandHistory.Add(command);
                        }


                        _historyIndex = _commandHistory.Count;
                        ViewModel.CommandInput = string.Empty;
                    }
                    e.Handled = true;
                    break;

                case VirtualKey.Up:
                    if (_commandHistory.Count > 0 && _historyIndex > 0)
                    {
                        _historyIndex--;
                        ViewModel.CommandInput = _commandHistory[_historyIndex];

                        textBox.SelectionStart = textBox.Text.Length;
                    }
                    e.Handled = true;
                    break;

                case VirtualKey.Down:
                    if (_historyIndex < _commandHistory.Count - 1)
                    {
                        _historyIndex++;
                        ViewModel.CommandInput = _commandHistory[_historyIndex];
                        textBox.SelectionStart = textBox.Text.Length;
                    }
                    else
                    {
                        _historyIndex = _commandHistory.Count;
                        ViewModel.CommandInput = string.Empty;
                    }
                    e.Handled = true;
                    break;
            }
        }
    }
}
