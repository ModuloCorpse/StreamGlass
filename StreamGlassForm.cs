using Quicksand.Web;
using Timer = System.Windows.Forms.Timer;

namespace StreamGlass
{
    public partial class StreamGlassForm : Form
    {
        private readonly static string CLIENT_ID = "azqkbuxql97vlhrfml3sbzklt3s4zg";
        private readonly Server m_WebServer = new();
        private readonly Twitch.Authenticator m_Authenticator;
        private TwitchBot? m_Bot = null;

        public StreamGlassForm()
        {
            InitializeComponent();
            m_WebServer.StartListening();

            m_Authenticator = new(m_WebServer, CLIENT_ID);
            Timer timer = new()
            {
                Interval = 50 //milliseconds
            };
            timer.Tick += StreamGlassForm_Tick;
            timer.Start();
        }

        private void StreamGlassForm_Tick(object? sender, EventArgs e)
        {
            m_WebServer.Update();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<string> scopes = new() { "channel:manage:polls", "channel:read:polls", "chat:read", "chat:edit", "channel:moderate" };
            string token = m_Authenticator.Authenticate(scopes);
            m_Bot = new("StreamGlass", token, "chaporon_");
        }
    }
}