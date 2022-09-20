using Quicksand.Web;
using System.Diagnostics;
using Timer = System.Windows.Forms.Timer;

namespace StreamGlass
{
    public partial class StreamGlassForm : Form
    {
        private readonly Stopwatch m_Watch = new();
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
            m_Bot.AddCommand("vod", "Vous avez raté un Live? Pas de soucis, retrouvez ChapORon sur YouTube : https://www.youtube.com/channel/UCLVOnS-3wkLXD7R8AzmkbLA");
            m_Bot.AddCommand("so", "Allez checker ${DisplayName(${1})}, qui joue à ${Game(${1})} sur https://twitch.tv/${Channel(${1})}", Twitch.IRC.UserMessage.UserType.MOD);
            m_Bot.AddCommand("discord", "Rejoignez le Discord de la Duck Army: https://discord.gg/FAMykZsCFN");

            m_Bot.AddTimedCommand(30 * 60 * 1000, 5, "vod");
            m_Bot.AddTimedCommand(15 * 60 * 1000, 5, "discord");

            Timer timer = new()
            {
                Interval = 50 //milliseconds
            };
            timer.Tick += StreamGlassForm_Tick;
            m_Watch.Start();
            timer.Start();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            m_Settings.Save();
        }

        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SettingsForm form = new(m_Settings, m_Bot);
            form.ShowDialog(this);
        }

        private void StreamGlassForm_Tick(object? sender, EventArgs e)
        {
            m_Watch.Stop();
            m_Bot.Update(m_Watch.ElapsedMilliseconds);
            m_Watch.Restart();
        }
    }
}