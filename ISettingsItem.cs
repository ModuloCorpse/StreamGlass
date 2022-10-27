using System.Windows.Controls;

namespace StreamGlass
{
    public interface ISettingsItem
    {
        public TabItem GetItem();
        public void OnSave();
    }
}
