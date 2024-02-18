﻿using CorpseLib.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using TabItem = StreamGlass.Core.Controls.TabItem;

namespace StreamGlass.Core.Settings
{
    public partial class Dialog : Controls.Dialog
    {
        private readonly List<TabItemContent> m_TabItems = [];

        public Dialog(Controls.Window window): base(window)
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
                    Source = ImageLoader.LoadStaticImage(item.GetHeaderSource())?.Source,
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
            OnOkClick();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (TabItemContent item in m_TabItems)
                item.CancelTabItem();
            OnCancelClick();
        }
    }
}
