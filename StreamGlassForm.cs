using Quicksand.Web;
using Timer = System.Windows.Forms.Timer;

namespace StreamGlass
{
    public partial class StreamGlassForm : Form
    {
        private readonly Settings m_Settings = new();
        private readonly Server m_WebServer = new();
        private readonly Twitch.Authenticator m_Authenticator;
        private TwitchBot? m_Bot = null;

        public StreamGlassForm()
        {
            InitializeComponent();
            m_WebServer.GetResourceManager().AddFramework();
            m_WebServer.Start();
            m_Settings.Load();

            m_Authenticator = new(m_WebServer, m_Settings.Twitch);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string token = m_Settings.Twitch.BotToken;
            if (string.IsNullOrWhiteSpace(token))
                token = m_Authenticator.Authenticate();
            m_Bot = new(m_Settings.Twitch.BotName, token, m_Settings.Twitch.Channel);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            m_Settings.Save();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SettingsForm form = new(m_Settings);
            form.ShowDialog(this);
        }
    }
}