using StreamGlass.Core.Controls;
using StreamGlass.Core.Events;
using System.Windows;
using TwitchCorpse;

namespace StreamGlass.StreamChat
{
    public partial class BanDialog : Dialog
    {
        private readonly TwitchUser m_UserToBan; 
        private BanEventArgs? m_Event = null;

        public BanDialog(Core.Controls.Window parent, TwitchUser userToBan) : base(parent)
        {
            InitializeComponent();
            m_UserToBan = userToBan;
            UserBannedLabel.Text = userToBan.DisplayName;
        }

        internal BanEventArgs? Event => m_Event;

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            m_Event = new(m_UserToBan, ReasonTextBox.Text, (uint)TimeUpDown.Value);
            OnOkClick();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            m_Event = null;
            OnCancelClick();
        }
    }
}
