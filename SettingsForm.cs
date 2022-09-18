namespace StreamGlass
{
    public partial class SettingsForm : Form
    {
        private readonly Settings m_Settings;

        public SettingsForm(Settings settings)
        {
            InitializeComponent();
            m_Settings = settings;

            //Twitch
            TwitchPublicTextBox.Text = m_Settings.Twitch.BotPublic;
            TwitchSecretTextBox.Text = m_Settings.Twitch.BotSecret;
            TwitchChannelTextBox.Text = m_Settings.Twitch.Channel;
            TwitchBotNameTextBox.Text = m_Settings.Twitch.BotName;
            TwitchBotTokenTextBox.Text = m_Settings.Twitch.BotToken;
            //Discord
            DiscordPublicTextBox.Text = m_Settings.Discord.BotPublic;
            DiscordSecretTextBox.Text = m_Settings.Discord.BotSecret;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            //Twitch
            m_Settings.Twitch.BotPublic = TwitchPublicTextBox.Text;
            m_Settings.Twitch.BotSecret = TwitchSecretTextBox.Text;
            m_Settings.Twitch.Channel = TwitchChannelTextBox.Text;
            m_Settings.Twitch.BotName = TwitchBotNameTextBox.Text;
            m_Settings.Twitch.BotToken = TwitchBotTokenTextBox.Text;
            //Discord
            m_Settings.Discord.BotPublic = DiscordPublicTextBox.Text;
            m_Settings.Discord.BotSecret = DiscordSecretTextBox.Text;
            Close();
        }

        private void TwitchSecretVisibilityButton_Click(object sender, EventArgs e)
        {
            if (TwitchBotTokenTextBox.PasswordChar == '*')
            {
                TwitchSecretTextBox.PasswordChar = '\0';
                TwitchSecretVisibilityButton.BackgroundImage = Properties.Resources.sight_enabled;
            }
            else
            {
                TwitchSecretTextBox.PasswordChar = '*';
                TwitchSecretVisibilityButton.BackgroundImage = Properties.Resources.sight_disabled;
            }
        }

        private void TwitchTokenVisibilityButton_Click(object sender, EventArgs e)
        {
            if (TwitchBotTokenTextBox.PasswordChar == '*')
            {
                TwitchBotTokenTextBox.PasswordChar = '\0';
                TwitchTokenVisibilityButton.BackgroundImage = Properties.Resources.sight_enabled;
            }
            else
            {
                TwitchBotTokenTextBox.PasswordChar = '*';
                TwitchTokenVisibilityButton.BackgroundImage = Properties.Resources.sight_disabled;
            }
        }

        private void DiscordSecretVisibilityButton_Click(object sender, EventArgs e)
        {
            if (DiscordSecretTextBox.PasswordChar == '*')
            {
                DiscordSecretTextBox.PasswordChar = '\0';
                DiscordSecretVisibilityButton.BackgroundImage = Properties.Resources.sight_enabled;
            }
            else
            {
                DiscordSecretTextBox.PasswordChar = '*';
                DiscordSecretVisibilityButton.BackgroundImage = Properties.Resources.sight_disabled;
            }
        }
    }
}
