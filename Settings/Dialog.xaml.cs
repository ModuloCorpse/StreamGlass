using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using TabItem = StreamGlass.Controls.TabItem;

namespace StreamGlass.Settings
{
    public partial class Dialog : StreamGlass.Controls.Dialog
    {
        private readonly List<TabItemContent> m_TabItems = new();

        public Dialog(StreamGlassWindow window): base(window)
        {
            InitializeComponent();
        }

        public void AddTabItem(TabItemContent item)
        {
            item.SetSettingDialog(this);
            item.UpdateTabItemColorPalette(GetBrushPalette());
            TabItem tabItem = new()
            {
                Header = new Image()
                {
                    Source = new BitmapImage(new Uri(item.GetHeaderSource(), UriKind.Relative)),
                    Width = 35,
                    Height = 35
                },
                Content = item
            };
            m_TabItems.Add(item);
            SettingsTabControl.Items.Add(tabItem);
        }

        public void AddTabItem(TabItemContent[] items)
        {
            foreach (TabItemContent item in items)
                AddTabItem(item);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (TabItemContent item in m_TabItems)
                item.SaveTabItem();
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (TabItemContent item in m_TabItems)
                item.CancelTabItem();
            Close();
        }
    }
}
