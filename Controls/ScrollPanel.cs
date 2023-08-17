using System.Windows.Controls;
using System.Windows;
using System.Collections.Generic;

namespace StreamGlass.Controls
{
    public class ScrollPanel<T> : UserControl where T : Control
    {
        private readonly Grid MainPanel = new();
        private readonly ScrollViewer Scroller = new();
        private readonly DockPanel ScrollableDockPanel = new();
        private readonly StackPanel ElementsStackPanel = new();
        private readonly List<T> m_Controls = new();
        private bool m_AutoScroll = true;
        private bool m_ForceAutoScroll = false;
        private bool m_Reversed = false;
        private bool m_IsOnBottom = false;

        public List<T> Controls => m_Controls;

        public ScrollPanel(): base()
        {
            SizeChanged += UserControl_SizeChanged;

            System.Windows.Controls.DockPanel.SetDock(ElementsStackPanel, Dock.Bottom);
            ElementsStackPanel.VerticalAlignment = VerticalAlignment.Top;
            ScrollableDockPanel.LastChildFill = false;
            ScrollableDockPanel.Children.Add(ElementsStackPanel);

            Scroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            Scroller.ScrollChanged += ChatScrollViewer_ScrollChanged;
            System.Windows.Controls.Grid.SetRow(Scroller, 0);

            Scroller.Content = ScrollableDockPanel;

            MainPanel.RowDefinitions.Add(new() { Height = new GridLength(1, GridUnitType.Star) });

            MainPanel.Children.Add(Scroller);

            Content = MainPanel;
        }

        public ScrollPanelDisplayType GetDisplayType()
        {
            if (m_IsOnBottom)
                return (m_Reversed) ? ScrollPanelDisplayType.REVERSED_BOTTOM_TO_TOP : ScrollPanelDisplayType.BOTTOM_TO_TOP;
            return (m_Reversed) ? ScrollPanelDisplayType.REVERSED_TOP_TO_BOTTOM : ScrollPanelDisplayType.TOP_TO_BOTTOM;
        }

        public void SetDisplayType(ScrollPanelDisplayType displayType)
        {
            switch (displayType)
            {
                case ScrollPanelDisplayType.TOP_TO_BOTTOM:
                    {
                        m_IsOnBottom = false;
                        m_Reversed = false;
                        break;
                    }
                case ScrollPanelDisplayType.REVERSED_TOP_TO_BOTTOM:
                    {
                        m_IsOnBottom = false;
                        m_Reversed = true;
                        break;
                    }
                case ScrollPanelDisplayType.BOTTOM_TO_TOP:
                    {
                        m_IsOnBottom = true;
                        m_Reversed = false;
                        break;
                    }
                case ScrollPanelDisplayType.REVERSED_BOTTOM_TO_TOP:
                    {
                        m_IsOnBottom = true;
                        m_Reversed = true;
                        break;
                    }
            }
            UpdateControlsPosition();
            UpdateScrollBar(false);
        }

        protected void UpdateControlsPosition()
        {
            if (m_IsOnBottom)
                System.Windows.Controls.DockPanel.SetDock(ElementsStackPanel, Dock.Bottom);
            else
                System.Windows.Controls.DockPanel.SetDock(ElementsStackPanel, Dock.Top);
            double chatPanelHeight = 0;
            ElementsStackPanel.Children.Clear();
            int i = (m_Reversed) ? m_Controls.Count - 1 : 0;
            int last = (m_Reversed) ? -1 : m_Controls.Count;
            int increment = (m_Reversed) ? -1 : 1;
            while (i != last)
            {
                T control = m_Controls[i];
                chatPanelHeight += control.ActualHeight;
                ElementsStackPanel.Children.Add(control);
                i += increment;
            }
            ScrollableDockPanel.Height = chatPanelHeight;
        }

        protected void AddControl(T control)
        {
            ElementsStackPanel.Children.Add(control);
            m_Controls.Add(control);
        }

        protected void RemoveControl(T control)
        {
            ElementsStackPanel.Children.Remove(control);
            m_Controls.Remove(control);
        }

        public void Add(T control)
        {
            AddControl(control);
            UpdateControlsPosition();
        }

        public void Remove(T control)
        {
            RemoveControl(control);
            UpdateControlsPosition();
        }

        public void Remove(IEnumerable<T> controls)
        {
            foreach (T control in controls)
                RemoveControl(control);
            UpdateControlsPosition();
        }

        public void Clear()
        {
            ElementsStackPanel.Children.Clear();
            m_Controls.Clear();
        }

        private void UpdateScrollBar(bool extentHeightChange)
        {
            if (m_IsOnBottom == m_Reversed)
            {
                if (!extentHeightChange)
                    m_AutoScroll = (Scroller.VerticalOffset == Scroller.ScrollableHeight);
                if ((m_AutoScroll && extentHeightChange) || m_ForceAutoScroll)
                    Scroller.ScrollToVerticalOffset(Scroller.ExtentHeight);
            }
            else
            {
                if (!extentHeightChange)
                    m_AutoScroll = (Scroller.VerticalOffset == 0);
                if ((m_AutoScroll && extentHeightChange) || m_ForceAutoScroll)
                    Scroller.ScrollToVerticalOffset(0);
            }
        }

        private void ChatScrollViewer_ScrollChanged(object? sender, ScrollChangedEventArgs e)
        {
            UpdateScrollBar(e.ExtentHeightChange != 0);
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ScrollableDockPanel.MinHeight = Scroller.ActualHeight;
            UpdateControlsPosition();
        }
    }
}
