using Quicksand.Web;

namespace StreamGlass
{
    public partial class StreamGlassForm : Form
    {
        private readonly Settings m_Settings = new();
        private readonly Server m_WebServer = new();
        private readonly TwitchBot m_Bot;

        public StreamGlassForm()
        {
            InitializeComponent();
            m_WebServer.GetResourceManager().AddFramework();
            m_WebServer.Start();
            m_Settings.Load();

            m_Bot = new(m_WebServer, m_Settings);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            m_Bot.Connect();
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