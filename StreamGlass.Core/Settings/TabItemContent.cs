using StreamGlass.Core.Controls;

namespace StreamGlass.Core.Settings
{
    public abstract class TabItemContent(string headerSource) : UserControl
    {
        private readonly string m_HeaderSource = headerSource;

        internal void Init() => OnInit();

        internal string GetHeaderSource() => m_HeaderSource;

        internal void SaveTabItem() => OnSave();

        internal void CancelTabItem() => OnCancel();

        protected virtual void OnInit() {}

        protected virtual void OnSave() {}

        protected virtual void OnCancel() {}
    }
}
