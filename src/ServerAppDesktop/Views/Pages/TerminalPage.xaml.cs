using System.Collections.Generic;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using ServerAppDesktop.ViewModels;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ServerAppDesktop.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TerminalPage : Page
    {
        private List<string> _commandHistory = new();
        private int _historyIndex = -1;
        public TerminalViewModel ViewModel => App.GetRequiredService<TerminalViewModel>();
        public TerminalPage()
        {
            InitializeComponent();
            ViewModel.TextChanged += () => { ScrollOutput.ChangeView(null, ScrollOutput.VerticalOffset, null); };
        }

        private void CommandInput_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null)
                return;

            switch (e.Key)
            {
                case VirtualKey.Enter:
                    string command = textBox.Text.Trim();
                    if (!string.IsNullOrWhiteSpace(command))
                    {
                        // 1. Enviar al proceso
                        ViewModel.SendInputCommand.Execute(command);

                        // 2. Guardar en el historial (solo si no es igual al último)
                        if (_commandHistory.Count == 0 || _commandHistory[^1] != command)
                        {
                            _commandHistory.Add(command);
                        }

                        // 3. Resetear índice y limpiar input
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
                        // Poner el cursor al final del texto
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
