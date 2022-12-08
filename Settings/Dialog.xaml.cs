using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using StreamFeedstock;
using StreamFeedstock.Controls;

namespace StreamGlass.Settings
{
    public partial class Dialog : StreamFeedstock.Controls.Dialog
    {
        private readonly List<TabItem> m_TabItems = new();

        public Dialog(StreamGlassWindow window): base(window)
        {
            InitializeComponent();
        }

        protected override void OnUpdate(BrushPaletteManager palette, TranslationManager translation)
        {
            foreach (TabItem item in m_TabItems)
                item.UpdateTabItemColorPalette(palette, translation);
        }

        public void AddTabItem(TabItem item)
        {
            item.SetSettingDialog(this);
            item.UpdateTabItemColorPalette(GetBrushPalette(), GetTranslations());
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

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (TabItem item in m_TabItems)
                item.CancelTabItem();
            Close();
        }
    }
}
