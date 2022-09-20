namespace StreamGlass
{
    public partial class SettingsForm : Form
    {
        private readonly Settings m_Settings;

        public SettingsForm(Settings settings)
        {
            InitializeComponent();
            m_Settings = settings;

            SystemBrowserTextBox.Text = m_Settings.Get("system", "browser");
            //Twitch
            TwitchChannelTextBox.Text = m_Settings.Get("twitch", "channel");
            TwitchBotPublicTextBox.Text = m_Settings.Get("twitch", "public_key");
            TwitchBotSecretTextBox.Text = m_Settings.Get("twitch", "secret_key");
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            m_Settings.Set("system", "browser", SystemBrowserTextBox.Text);
            //Twitch
            m_Settings.Set("twitch", "channel", TwitchChannelTextBox.Text);
            m_Settings.Set("twitch", "public_key", TwitchBotPublicTextBox.Text);
            m_Settings.Set("twitch", "secret_key", TwitchBotSecretTextBox.Text);
            Close();
        }

        private void SystemBrowserFileDialog_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                InitialDirectory = "C:\\",
                Filter = "exe files (*.exe)|*.exe|All files (*.*)|*.*",
                FilterIndex = 2,
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
                SystemBrowserTextBox.Text = openFileDialog.FileName;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (TwitchBotSecretTextBox.PasswordChar == '*')
            {
                TwitchBotSecretTextBox.PasswordChar = '\0';
                button1.BackgroundImage = Properties.Resources.sight_enabled;
            }
            else
            {
                TwitchBotSecretTextBox.PasswordChar = '*';
                button1.BackgroundImage = Properties.Resources.sight_disabled;
            }
        }
    }
}
