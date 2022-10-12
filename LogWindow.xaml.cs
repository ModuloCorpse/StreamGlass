using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace StreamGlass
{
    /// <summary>
    /// Interaction logic for LogWindow.xaml
    /// </summary>
    public partial class LogWindow : Window
    {
        private bool m_IsRefreshingComboBox = false;
        private string m_CurrentLogCategory = "";
        private readonly Dictionary<string, List<string>> m_Logs = new();
        private object m_Lock = new();

        public LogWindow()
        {
            InitializeComponent();
        }

        public void SetCurrentLogCategory(string category)
        {
            m_CurrentLogCategory = category;
            Dispatcher.Invoke(() => UpdateLogListView());
        }

        public void Log(string category, string log)
        {
            lock (m_Lock)
            {
                if (m_Logs.TryGetValue(category, out List<string>? logs))
                    logs.Add(log);
                else
                    m_Logs.Add(category, new() { log });
            }
            Dispatcher.Invoke(() => UpdateLogListView());
        }

        private void UpdateLogListView()
        {
            lock (m_Lock)
            {
                m_IsRefreshingComboBox = true;
                LogDisplayComboBox.Items.Clear();
                foreach (string category in m_Logs.Keys)
                {
                    if (category == m_CurrentLogCategory)
                        LogDisplayComboBox.SelectedIndex = LogDisplayComboBox.Items.Count;
                    LogDisplayComboBox.Items.Add(category);
                }
                m_IsRefreshingComboBox = false;

                LogsListView.Items.Clear();
                if (m_Logs.TryGetValue(m_CurrentLogCategory, out List<string>? logs))
                {
                    foreach (string log in logs)
                        LogsListView.Items.Add(log);
                }
            }
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

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}
