using CorpseLib;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace StreamGlass.Core.Controls
{
    //TODO Switch to XAML
    public class EditableList : UserControl
    {
        public delegate string ConversionDelegate(object obj);

        private class ItemWrapper(object item, ConversionDelegate? conversionDelegate)
        {
            private readonly object m_Item = item;
            private readonly ConversionDelegate? m_ConversionDelegate = conversionDelegate;

            public object Item => m_Item;

            public override bool Equals(object? obj) => obj is ItemWrapper other ? m_Item!.Equals(other.m_Item) : m_Item!.Equals(obj);
            public override int GetHashCode() => m_Item!.GetHashCode();
            public override string ToString() => m_ConversionDelegate != null ? m_ConversionDelegate(m_Item) : m_Item!.ToString()!;
        }

        #region IsSearchable
        public static readonly DependencyProperty IsSearchableProperty = Helper.NewProperty("IsSearchable", true, (EditableList instance, bool value) => instance.Property_SetIsSearchable(value));
        [Description("Specify if user can search element"), Category("Common Properties")]
        public bool IsSearchable { get => (bool)GetValue(IsSearchableProperty); set => SetValue(IsSearchableProperty, value); }
        internal void Property_SetIsSearchable(bool value)
        {
            if (value)
                SearchPanel.Visibility = Visibility.Visible;
            else
                SearchPanel.Visibility = Visibility.Collapsed;
        }
        #endregion IsSearchable

        #region IsEditOnly
        public static readonly DependencyProperty IsEditOnlyProperty = Helper.NewProperty("IsEditOnly", false, (EditableList instance, bool value) => instance.Property_SetIsEditOnly(value));
        [Description("Specify if user can add/remove element"), Category("Common Properties")]
        public bool IsEditOnly { get => (bool)GetValue(IsEditOnlyProperty); set => SetValue(IsEditOnlyProperty, value); }
        internal void Property_SetIsEditOnly(bool value)
        {
            if (value)
            {
                AddButton.Visibility = Visibility.Collapsed;
                RemoveButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                AddButton.Visibility = Visibility.Visible;
                RemoveButton.Visibility = Visibility.Visible;
            }
        }
        #endregion IsEditOnly

        #region SearchImageSource
        public static readonly DependencyProperty SearchImageSourceProperty = Helper.NewProperty("SearchImageSource", string.Empty, (EditableList instance, string value) => instance.Property_SetSearchImageSource(value));
        [Description("Image of the search button"), Category("Common Properties")]
        public string SearchImageSource { get => (string)GetValue(SearchImageSourceProperty); set => SetValue(SearchImageSourceProperty, value); }
        internal void Property_SetSearchImageSource(string source)
        {
            if (!string.IsNullOrWhiteSpace(source))
            {
                SearchButton.Content = new Image
                {
                    Source = new BitmapImage(new Uri(source, UriKind.RelativeOrAbsolute)),
                    Height = 20,
                    Width = 20
                };
            }
        }
        #endregion SearchImageSource

        #region EditImageSource
        public static readonly DependencyProperty EditImageSourceProperty = Helper.NewProperty("EditImageSource", string.Empty, (EditableList instance, string value) => instance.Property_SetEditImageSource(value));
        [Description("Image of the edit button"), Category("Common Properties")]
        public string EditImageSource { get => (string)GetValue(EditImageSourceProperty); set => SetValue(EditImageSourceProperty, value); }
        internal void Property_SetEditImageSource(string source)
        {
            if (!string.IsNullOrWhiteSpace(source))
            {
                EditButton.Content = new Image
                {
                    Source = new BitmapImage(new Uri(source, UriKind.RelativeOrAbsolute)),
                    Height = 20,
                    Width = 20
                };
            }
        }
        #endregion EditImageSource

        #region BrushPaletteKey
        public static readonly DependencyProperty BrushPaletteKeyProperty = Helper.NewProperty("BrushPaletteKey", "background", (EditableList i, string v) => i.SetBrushPaletteKey(v));
        [Description("The brush palette key of the editable list's background"), Category("Common Properties")]
        public string BrushPaletteKey
        {
            get => (string)GetValue(BrushPaletteKeyProperty);
            set => SetValue(BrushPaletteKeyProperty, value);
        }
        internal void SetBrushPaletteKey(string key)
        {
            ListDockPanel.BrushPaletteKey = key;
            ButtonPanel.BrushPaletteKey = key;
            SearchPanel.BrushPaletteKey = key;
        }
        #endregion BrushPaletteKey

        #region ListBrushPaletteKey
        public static readonly DependencyProperty ListBrushPaletteKeyProperty = Helper.NewProperty("ListBrushPaletteKey", "background_2", (EditableList i, string v) => i.SetListBrushPaletteKey(v));
        [Description("The brush palette key of the list view's background"), Category("Common Properties")]
        public string ListBrushPaletteKey
        {
            get => (string)GetValue(ListBrushPaletteKeyProperty);
            set => SetValue(ListBrushPaletteKeyProperty, value);
        }
        internal void SetListBrushPaletteKey(string key) => ObjectListBox.BrushPaletteKey = key;
        #endregion BrushPaletteKey

        #region TextBrushPaletteKey
        public static readonly DependencyProperty TextBrushPaletteKeyProperty = Helper.NewProperty("TextBrushPaletteKey", "text", (EditableList i, string v) => i.SetTextBrushPaletteKey(v));
        [Description("The brush palette key of the list view's text"), Category("Common Properties")]
        public string TextBrushPaletteKey
        {
            get => (string)GetValue(TextBrushPaletteKeyProperty);
            set => SetValue(TextBrushPaletteKeyProperty, value);
        }
        internal void SetTextBrushPaletteKey(string key) => ObjectListBox.TextBrushPaletteKey = key;
        #endregion TextBrushPaletteKey

        private readonly DockPanel ListDockPanel = new() { LastChildFill = true, BrushPaletteKey = "list_background" };
        private readonly StackPanel ButtonPanel = new();
        private readonly Button AddButton = new() { Content = "+", Width = 20, Height = 20, BrushPaletteKey = "list_add" };
        private readonly Button RemoveButton = new() { Content = "-", Width = 20, Height = 20, Margin = new(0, 5, 0, 0), BrushPaletteKey = "list_remove" };
        private readonly Button EditButton = new() { Width = 20, Height = 20, Margin = new(0, 5, 0, 0), BrushPaletteKey = "list_edit" };
        private readonly StackPanel SearchPanel = new() { Margin = new Thickness(0, 0, 0, 3), Orientation = Orientation.Horizontal };
        private readonly TextBox SearchTextBox = new() { Width = 200 };
        private readonly Button SearchButton = new() { Width = 20, Height = 20, BrushPaletteKey = "list_search" };
        private readonly ListBox ObjectListBox = new() { Margin = new(0, 0, 5, 0), BrushPaletteKey = "list_item_background", TextBrushPaletteKey = "list_item_text" };
        private readonly DictionaryTree<ItemWrapper> m_SearchTree = new();
        private string m_Search = string.Empty;

        public event EventHandler? ItemAdded;
        public event EventHandler<object>? ItemRemoved;
        public event EventHandler<object>? ItemEdited;

        private ConversionDelegate? m_ConversionDelegate = null;

        public EditableList()
        {
            System.Windows.Controls.DockPanel.SetDock(ButtonPanel, Dock.Right);
            System.Windows.Controls.DockPanel.SetDock(ObjectListBox, Dock.Left);
            System.Windows.Controls.DockPanel.SetDock(SearchPanel, Dock.Top);
            AddButton.Click += AddButton_Click;
            RemoveButton.Click += RemoveButton_Click;
            EditButton.Click += EditButton_Click;
            ButtonPanel.Children.Add(AddButton);
            ButtonPanel.Children.Add(RemoveButton);
            ButtonPanel.Children.Add(EditButton);
            SearchTextBox.Text = m_Search;
//            SearchButton.Click += SearchButton_Click;
            SearchTextBox.TextChanged += SearchTextBox_TextChanged;
            SearchPanel.Children.Add(SearchTextBox);
//            SearchPanel.Children.Add(SearchButton);
            ListDockPanel.Children.Add(SearchPanel);
            ListDockPanel.Children.Add(ButtonPanel);
            ListDockPanel.Children.Add(ObjectListBox);
            Content = ListDockPanel;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            m_Search = SearchTextBox.Text;
            UpdateList();
        }

        public void SetConversionDelegate(ConversionDelegate conversionDelegate) => m_ConversionDelegate = conversionDelegate;

        private void UpdateList()
        {
            ObjectListBox.Items.Clear();
            List<ItemWrapper> list = m_SearchTree.Search(m_Search);
            foreach (ItemWrapper item in list)
                ObjectListBox.Items.Add(item);
            ObjectListBox.Items.Refresh();
        }

        public void UpdateObject(object oldObj, object newObj)
        {
            for (int i = 0; i != ObjectListBox.Items.Count; ++i)
            {
                ItemWrapper? oldWrapper = (ItemWrapper?)ObjectListBox.Items[i];
                if (oldWrapper != null && oldWrapper.Equals(oldObj))
                {
                    m_SearchTree.Remove(oldWrapper.ToString());
                    AddObject(newObj);
                    return;
                }
            }
        }

        public void AddObjects(IEnumerable<object> items)
        {
            foreach (object item in items)
            {
                ItemWrapper itemWrapper = new(item, m_ConversionDelegate);
                m_SearchTree.Add(itemWrapper.ToString(), itemWrapper);
            }
            UpdateList();
        }

        public void AddObject(object obj)
        {
            ItemWrapper itemWrapper = new(obj, m_ConversionDelegate);
            m_SearchTree.Add(itemWrapper.ToString(), itemWrapper);
            UpdateList();
        }

        public void Clear()
        {
            m_SearchTree.Clear();
            UpdateList();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e) => ItemAdded?.Invoke(this, EventArgs.Empty);

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            ItemWrapper? item = (ItemWrapper?)ObjectListBox.SelectedItem;
            if (item != null)
            {
                m_SearchTree.Remove(item.ToString());
                UpdateList();
                ItemRemoved?.Invoke(this, item.Item);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            ItemWrapper? item = (ItemWrapper?)ObjectListBox.SelectedItem;
            if (item != null)
                ItemEdited?.Invoke(this, item.Item);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            m_Search = SearchTextBox.Text;
            UpdateList();
        }

        public List<object> GetItems()
        {
            List<object> ret = [];
            foreach (var item in ObjectListBox.Items)
            {
                if (item != null && item is ItemWrapper wrapper)
                    ret.Add(wrapper.Item);
            }
            return ret;
        }
    }
}
