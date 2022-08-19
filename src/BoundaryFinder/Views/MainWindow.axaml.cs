using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace BoundaryFinder.Views
{
    public partial class MainWindow : Window
    {
        // HACK: This stuff about hiding and showing the log panel is not exactly idiomatic
        private bool _suppressHeightChanged = false;
        
        public MainWindow()
        {
            InitializeComponent();
            GridLayout.RowDefinitions[2].PropertyChanged += LogRowHeightChanged;
        }

        private void LogRowHeightChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (!_suppressHeightChanged)
            {
                LogBox.IsVisible = true;
            }
        }

        private void CloseButtonClicked(object? sender, RoutedEventArgs e)
        {
            _suppressHeightChanged = true;
            LogBox.IsVisible = false;
            GridLayout.RowDefinitions[2].Height = new GridLength(0);
            _suppressHeightChanged = false;
        }
    }
}