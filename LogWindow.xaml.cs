using StreamGlass.UI;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StreamGlass
{
    public partial class LogWindow : UI.Dialog
    {
        private class ListViewLog : System.Windows.Controls.ListViewItem, IUIElement
        {
            public void Update(BrushPaletteManager palette, TranslationManager translation)
            {
                if (palette.TryGetColor("background_2", out var background))
                    Background = background;
                if (palette.TryGetColor("text", out var foreground))
                    Foreground = foreground;
                if (ContextMenu is IUIElement menu)
                    menu.Update(palette, translation);
            }
        }

        private bool m_IsRefreshingComboBox = false;
        private string m_CurrentLogCategory = "";

        public LogWindow(UI.Window parent): base(parent)
        {
            InitializeComponent();
            Logger.SetLogWindow(this);
            UpdateLogListView();
        }

        public void SetCurrentLogCategory(string category)
        {
            m_CurrentLogCategory = category;
            UpdateLogs();
        }

        public void UpdateLogs() => Dispatcher.Invoke(() => UpdateLogListView());

        private void UpdateLogListView()
        {
            m_IsRefreshingComboBox = true;
            LogDisplayComboBox.Items.Clear();
            foreach (string category in Logger.GetCategories())
            {
                if (category == m_CurrentLogCategory)
                    LogDisplayComboBox.SelectedIndex = LogDisplayComboBox.Items.Count;
                LogDisplayComboBox.Items.Add(category);
            }
            m_IsRefreshingComboBox = false;

            LogsListView.Items.Clear();
            foreach (string log in Logger.GetLogs(m_CurrentLogCategory))
            {
                UI.ContextMenu contextMenu = new();
                UI.MenuItem menuItem = new() { Header = "Copy" };
                menuItem.Click += MenuItem_Click;
                contextMenu.Items.Add(menuItem);
                LogsListView.Items.Add(new ListViewLog() { Content = log, ContextMenu = contextMenu });
            }
            LogsListView.Update(GetBrushPalette(), GetTranslations());
        }

        private void LogDisplayComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m_IsRefreshingComboBox)
                return;
            if (LogDisplayComboBox.SelectedItem != null && LogDisplayComboBox == sender)
            {
                m_CurrentLogCategory = (string)LogDisplayComboBox.SelectedItem;
                UpdateLogListView();
            }
        }

        protected override void OnClosing(CancelEventArgs e) => Logger.SetLogWindow(null);

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            UI.MenuItem menuItem = (UI.MenuItem)sender;
            UI.ContextMenu contextMenu = (UI.ContextMenu)menuItem.Parent;
            ListViewItem item = (ListViewItem)contextMenu.PlacementTarget;
            Clipboard.SetText(item.Content.ToString());
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }
    }
}
