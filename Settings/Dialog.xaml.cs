using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace StreamGlass.Settings
{
    public partial class Dialog : Window
    {
        private readonly List<TabItem> m_TabItems = new();

        public Dialog()
        {
            InitializeComponent();
        }

        public void AddTabItem(TabItem item)
        {
            System.Windows.Controls.TabItem tabItem = new();
            tabItem.Header = new Image()
            {
                Source = new BitmapImage(new Uri(item.GetHeaderSource(), UriKind.Relative)),
                Width = 35,
                Height = 35
            };
            tabItem.Content = item;
            m_TabItems.Add(item);
            SettingsTabControl.Items.Add(tabItem);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (TabItem item in m_TabItems)
                item.SaveTabItem();
            Close();
        }
    }
}
