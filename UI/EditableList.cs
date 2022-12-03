using System.Windows.Controls;
using System.Windows;
using System;
using System.Collections.Generic;
using StreamGlass.ManagedObject;

namespace StreamGlass.UI
{
    public class EditableList : UserControl
    {
        public delegate string ConversionDelegate(object obj);

        private class ItemWrapper
        {
            private readonly object m_Item;
            private readonly ConversionDelegate? m_ConversionDelegate;

            public object Item => m_Item;

            public ItemWrapper(object item, ConversionDelegate? conversionDelegate)
            {
                m_Item = item;
                m_ConversionDelegate = conversionDelegate;
            }

            public override bool Equals(object? obj) => obj is ItemWrapper other ? m_Item!.Equals(other.m_Item) : m_Item!.Equals(obj);
            public override int GetHashCode() => m_Item!.GetHashCode();
            public override string ToString() => m_ConversionDelegate != null ? m_ConversionDelegate(m_Item) : m_Item!.ToString()!;
        }

        private readonly DockPanel ListDockPanel = new() { LastChildFill = true, BrushPaletteKey = "list_background" };
        private readonly StackPanel ButtonPanel = new();
        private readonly Button AddButton = new() { Content = "+", Width = 20, Height = 20, BrushPaletteKey = "list_add" };
        private readonly Button RemoveButton = new() { Content = "-", Width = 20, Height = 20, Margin = new(0, 5, 0, 0), BrushPaletteKey = "list_remove" };
        private readonly Button EditButton = new() { Width = 20, Height = 20, Margin = new(0, 5, 0, 0), BrushPaletteKey = "list_edit" };
        private readonly ListBox ObjectListBox = new() { Margin = new(0, 0, 5, 0), BrushPaletteKey = "list_item_background", TextBrushPaletteKey = "list_item_text" };
        private readonly SearchTree<ItemWrapper> m_SearchTree = new();
        private string m_Search = "";

        public event EventHandler? ItemAdded;
        public event EventHandler<object>? ItemRemoved;
        public event EventHandler<object>? ItemEdited;

        private ConversionDelegate? m_ConversionDelegate = null;

        public EditableList()
        {
            System.Windows.Controls.DockPanel.SetDock(ButtonPanel, Dock.Right);
            AddButton.Click += AddButton_Click;
            RemoveButton.Click += RemoveButton_Click;
            EditButton.Click += EditButton_Click;
            ButtonPanel.Children.Add(AddButton);
            ButtonPanel.Children.Add(RemoveButton);
            ButtonPanel.Children.Add(EditButton);
            ListDockPanel.Children.Add(ButtonPanel);
            ListDockPanel.Children.Add(ObjectListBox);
            Content = ListDockPanel;
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
                AddObject(item);
        }

        public void AddObject(object obj)
        {
            ItemWrapper itemWrapper = new(obj, m_ConversionDelegate);
            m_SearchTree.Add(itemWrapper.ToString(), itemWrapper);
            UpdateList();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            ItemAdded?.Invoke(this, EventArgs.Empty);
        }

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

        public List<object> GetItems()
        {
            List<object> ret = new();
            foreach (var item in ObjectListBox.Items)
            {
                if (item != null && item is ItemWrapper wrapper)
                    ret.Add(wrapper.Item);
            }
            return ret;
        }
    }
}
