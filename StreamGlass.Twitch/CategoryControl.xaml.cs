using StreamGlass.Core.Controls;
using System.Windows;
using System.Windows.Media.Imaging;
using TwitchCorpse.API;

namespace StreamGlass.Twitch
{
    public partial class CategoryControl : UserControl
    {
        private readonly CategorySearchDialog m_Dialog;
        private readonly TwitchCategoryInfo m_Info;
        private bool m_IsForced = false;

        public string CategoryID => m_Info.ID;
        public string CategoryName => m_Info.Name;

        public CategoryControl(CategorySearchDialog dialog, TwitchCategoryInfo category)
        {
            m_Dialog = dialog;
            m_Info = category;
            InitializeComponent();
            BitmapImage bitmap = new();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(category.ImageURL, UriKind.RelativeOrAbsolute);
            bitmap.EndInit();
            CategoryImage.Source = bitmap;
            CategoryNameLabel.Text = category.Name;
        }

        public void ForceUncheck() => CategorySelection.IsChecked = false;

        public void SetChecked(bool value)
        {
            m_IsForced = true;
            CategorySelection.IsChecked = value;
            m_IsForced = false;
        }

        private void CategorySelection_Checked(object sender, RoutedEventArgs e)
        {
            if (!m_IsForced)
                m_Dialog.SetCurrentControl(this);
        }

        private void CategorySelection_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!m_IsForced)
                m_Dialog.SetCurrentControl(null);
        }
    }
}
