using StreamGlass.Controls;
using System.Collections.Generic;
using System.Windows;
using TwitchCorpse;

namespace StreamGlass.Twitch
{
    public partial class CategorySearchDialog : Dialog
    {
        private readonly TwitchAPI m_API;
        private Profile.CategoryInfo? m_SearchedCategoryInfo = null;
        private readonly List<CategoryControl> m_Categories = [];
        private string m_CategoryID = string.Empty;
        private string m_CategoryName = string.Empty;

        public CategorySearchDialog(Controls.Window parent, Profile.CategoryInfo? info, TwitchAPI api) : base(parent)
        {
            m_API = api;
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
            m_CategoryID = currentControl?.CategoryID ?? string.Empty;
            m_CategoryName = currentControl?.CategoryName ?? string.Empty;
        }

        private void StreamInfoCategorySearchButton_Click(object sender, RoutedEventArgs e)
        {
            List<TwitchCategoryInfo> categories = m_API.SearchCategoryInfo(SearchFieldTextBox.Text);
            bool clearSelection = true;
            SearchResultsPanel.Children.Clear();
            m_Categories.Clear();
            foreach (TwitchCategoryInfo categoryInfo in categories)
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
                m_CategoryID = string.Empty;
                m_CategoryName = string.Empty;
            }
        }
    }
}
