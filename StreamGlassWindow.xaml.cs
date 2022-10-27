using Quicksand.Web;
using StreamGlass.Command;
using StreamGlass.StreamChat;
using StreamGlass.Twitch;
using System;
using System.Diagnostics;
using System.Windows;

namespace StreamGlass
{
    public partial class StreamGlassWindow : Window
    {
        private readonly LogWindow m_LogWindow = new();
        private readonly Stopwatch m_Watch = new();
        private readonly Settings.Data m_Settings = new();
        private readonly Server m_WebServer = new(); //To stop
        private readonly ProfileManager m_Manager;
        private readonly Bot m_Bot; //TODO: Change Twitch specific code to generic code

        public StreamGlassWindow()
        {
            CanalManager.NewCanal<UserMessage>(StreamGlassCanals.CHAT_MESSAGE);
            CanalManager.NewCanal(StreamGlassCanals.CHAT_CONNECTED);
            CanalManager.NewCanal<string>(StreamGlassCanals.CHAT_JOINED);
            CanalManager.NewCanal<string>(StreamGlassCanals.USER_JOINED);
            CanalManager.NewCanal<UpdateStreamInfoArgs>(StreamGlassCanals.UPDATE_STREAM_INFO);
            Logger.InitLogWindow(m_LogWindow);
            Logger.SetCurrentLogCategory("Twitch IRC");
            ChatCommand.Init();
            m_Settings.Load();
            InitializeComponent();
            m_WebServer.GetResourceManager().AddFramework();
            m_WebServer.Start();

            m_Bot = new(m_WebServer, m_Settings, this); //TODO: Change Twitch specific code to generic code
            m_Manager = new(m_Bot);
            m_Manager.Load();
            m_Manager.FillComboBox(ref CommandProfilesComboBox);

            System.Windows.Threading.DispatcherTimer dispatcherTimer = new();
            dispatcherTimer.Tick += StreamGlassForm_Tick;
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(50);
            m_Watch.Start();
            dispatcherTimer.Start();

            StreamChatPanel.SetStreamChat(m_Bot);
        }

        private void CommandProfilesComboBox_SelectionChanged(object sender, EventArgs e)
        {
            Profile.Info selector = (Profile.Info)CommandProfilesComboBox.SelectedItem;
            m_Manager.SetCurrentProfile(selector.ID);
            m_Manager.UpdateStreamInfo();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            m_Settings.Save();
            m_Manager.Save();
            m_Bot.Disconnect(); //TODO: Change Twitch specific code to generic code
            m_Watch.Stop();
            m_LogWindow.Close();
        }

        internal void JoinChannel(string channel)
        {
            m_Manager.UpdateStreamInfo();
            StreamChatPanel.AddChannel(channel);
            StreamChatPanel.SetChannel(channel);
        }

        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.Dialog dialog = new()
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            dialog.AddTabItem(new Twitch.SettingsItem(m_Settings, m_Bot)); //TODO: Change Twitch specific code to generic code
            dialog.Show();
        }

        private void LogsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_LogWindow.Show();
        }

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
