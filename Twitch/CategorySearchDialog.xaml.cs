using System.Collections.Generic;
using System.Windows;
using StreamFeedstock.Controls;

namespace StreamGlass.Twitch
{
    public partial class CategorySearchDialog : Dialog
    {
        private Profile.CategoryInfo? m_SearchedCategoryInfo = null;
        private string m_CategoryID = "";
        private string m_CategoryName = "";
        private readonly List<CategoryControl> m_Categories = new();

        public CategorySearchDialog(StreamFeedstock.Controls.Window parent, Profile.CategoryInfo? info): base(parent)
        {
            InitializeComponent();
            if (info != null)
            {
                m_CategoryID = info.ID;
                m_CategoryName = info.Name;
                SearchFieldTextBox.Text = info.Name;
            }
        }

        internal Profile.CategoryInfo? CategoryInfo => m_SearchedCategoryInfo;

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            m_SearchedCategoryInfo = new(m_CategoryID, m_CategoryName);
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            m_SearchedCategoryInfo = null;
            Close();
        }

        internal void SetCurrentControl(CategoryControl? currentControl)
        {
            foreach (CategoryControl control in m_Categories)
            {
                if (currentControl == null || control.CategoryID != currentControl.CategoryID)
                    control.SetChecked(false);
            }
            m_CategoryID = currentControl?.CategoryID ?? "";
            m_CategoryName = currentControl?.CategoryName ?? "";
        }

        private void StreamInfoCategorySearchButton_Click(object sender, RoutedEventArgs e)
        {
            List<CategoryInfo> categories = API.SearchCategoryInfo(SearchFieldTextBox.Text);
            bool clearSelection = true;
            SearchResultsPanel.Children.Clear();
            m_Categories.Clear();
            foreach (CategoryInfo categoryInfo in categories)
            {
                CategoryControl newControl = new(this, categoryInfo);
                m_Categories.Add(newControl);
                SearchResultsPanel.Children.Add(newControl);
                if (categoryInfo.ID == m_CategoryID)
                {
                    clearSelection = false;
                    newControl.SetChecked(true);
                }
            }
            if (clearSelection)
            {
                m_CategoryID = "";
                m_CategoryName = "";
            }
        }
    }
}
