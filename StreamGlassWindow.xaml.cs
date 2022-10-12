using Quicksand.Web;
using StreamGlass.Command;
using StreamGlass.Twitch;
using StreamGlass.Twitch.IRC;
using System;
using System.Diagnostics;
using System.Windows;

namespace StreamGlass
{
    /// <summary>
    /// Interaction logic for StreamGlassWindow.xaml
    /// </summary>
    public partial class StreamGlassWindow : Window
    {
        private ChannelInfo? m_OriginalBroadcasterChannelInfo = null;
        private readonly LogWindow m_LogWindow = new();
        private readonly Stopwatch m_Watch = new();
        private readonly Settings m_Settings = new();
        private readonly Server m_WebServer = new(); //To stop
        private readonly ProfileManager m_Manager;
        private readonly Twitch.IRC.Client m_Client = new("irc.chat.twitch.tv", 6697, true);
        private readonly TwitchBot m_Bot;

        public StreamGlassWindow()
        {
            Logger.InitLogWindow(m_LogWindow);
            Logger.SetCurrentLogCategory("Twitch IRC");
            m_Manager = new(m_Client);
            Profile.Init();
            m_Settings.Load();
            m_Manager.Load();
            InitializeComponent();
            m_WebServer.GetResourceManager().AddFramework();
            m_WebServer.Start();

            m_Bot = new(m_WebServer, m_Settings, m_Manager, m_Client, this);
            m_Manager.FillComboBox(ref CommandProfilesComboBox);

            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += StreamGlassForm_Tick;
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(50);
            m_Watch.Start();
            dispatcherTimer.Start();

            if (m_Settings.Get("twitch", "auto_connect") == "true")
                m_Bot.Connect();
        }

        private void CommandProfilesComboBox_SelectionChanged(object sender, EventArgs e)
        {
            Profile.Info selector = (Profile.Info)CommandProfilesComboBox.SelectedItem;
            m_Manager.SetCurrentProfile(selector.ID);
            if (m_OriginalBroadcasterChannelInfo != null)
                m_Manager.UpdateStreamInfo(m_OriginalBroadcasterChannelInfo.Broadcaster.ID);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            m_Settings.Save();
            m_Manager.Save();
            m_Client.Disconnect();
            m_Watch.Stop();
            m_LogWindow.Close();
            if (m_OriginalBroadcasterChannelInfo != null)
                API.SetChannelInfoFromID(m_OriginalBroadcasterChannelInfo.Broadcaster.ID,
                    m_OriginalBroadcasterChannelInfo.Title,
                    m_OriginalBroadcasterChannelInfo.GameID,
                    m_OriginalBroadcasterChannelInfo.BroadcasterLanguage);
        }

        internal void JoinChannel(string channel)
        {
            if (m_OriginalBroadcasterChannelInfo != null)
                API.SetChannelInfoFromID(m_OriginalBroadcasterChannelInfo.Broadcaster.ID,
                    m_OriginalBroadcasterChannelInfo.Title,
                    m_OriginalBroadcasterChannelInfo.GameID,
                    m_OriginalBroadcasterChannelInfo.BroadcasterLanguage);
            m_OriginalBroadcasterChannelInfo = API.GetChannelInfoFromLogin(channel[1..]);
            if (m_OriginalBroadcasterChannelInfo != null)
                m_Manager.UpdateStreamInfo(m_OriginalBroadcasterChannelInfo.Broadcaster.ID);
            StreamChatPanel.AddChannel(channel);
            StreamChatPanel.SetChannel(channel);
        }

        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SettingsDialog dialog = new(m_Settings, m_Bot)
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            dialog.Show();
        }

        private void LogsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_LogWindow.Show();
        }

        internal void AddMessage(UserMessage message) => StreamChatPanel.AddMessage(message);

        private void StreamGlassForm_Tick(object? sender, EventArgs e)
        {
            m_Watch.Stop();
            long deltaTime = m_Watch.ElapsedMilliseconds;
            m_Bot.Update(deltaTime);
            m_Manager.Update(deltaTime);
            m_Watch.Restart();
        }
    }
}
