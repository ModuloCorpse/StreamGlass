using Quicksand.Web;
using StreamGlass.Command;
using StreamGlass.StreamChat;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace StreamGlass
{
    public partial class StreamGlassWindow : Window
    {
        private readonly LogWindow m_LogWindow = new();
        private readonly Stopwatch m_Watch = new();
        private readonly Settings.Data m_Settings = new();
        private readonly Server m_WebServer = new();
        private readonly ProfileManager m_Manager;
        private readonly ConnectionManager m_ConnectionManager = new();

        public StreamGlassWindow()
        {
            CanalManager.NewCanal<UserMessage>(StreamGlassCanals.CHAT_MESSAGE);
            CanalManager.NewCanal(StreamGlassCanals.CHAT_CONNECTED);
            CanalManager.NewCanal<string>(StreamGlassCanals.CHAT_JOINED);
            CanalManager.NewCanal<string>(StreamGlassCanals.USER_JOINED);
            CanalManager.NewCanal<UpdateStreamInfoArgs>(StreamGlassCanals.UPDATE_STREAM_INFO);
            CanalManager.NewCanal(StreamGlassCanals.STREAM_START);
            Logger.InitLogWindow(m_LogWindow);
            Logger.SetCurrentLogCategory("Twitch IRC");
            ChatCommand.Init();
            m_Settings.Load();
            InitializeComponent();
            m_WebServer.GetResourceManager().AddFramework();
            m_WebServer.Start();

            m_ConnectionManager.RegisterConnection(new Twitch.Bot(m_WebServer, m_Settings, this));
            m_Manager = new(m_ConnectionManager);
            m_Manager.Load();
            m_Manager.FillComboBox(ref CommandProfilesComboBox);

            System.Windows.Threading.DispatcherTimer dispatcherTimer = new();
            dispatcherTimer.Tick += StreamGlassForm_Tick;
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(50);
            m_Watch.Start();
            dispatcherTimer.Start();

            StreamChatPanel.SetStreamChat(m_ConnectionManager);
        }

        private void CommandProfilesComboBox_SelectionChanged(object sender, EventArgs e)
        {
            Profile.Info selector = (Profile.Info)CommandProfilesComboBox.SelectedItem;
            m_Manager.SetCurrentProfile(selector.ID);
            m_Manager.UpdateStreamInfo();
        }

        public void SetChatPanelOnLeft() => SetColumns(0, 1);
        public void SetChatPanelOnRight() => SetColumns(1, 0);

        private void SetColumns(int chatColumn, int profileColumn)
        {
            Grid.SetColumn(StreamChatPanel, chatColumn);
            MainGrid.ColumnDefinitions[chatColumn].Width = new GridLength(2.2, GridUnitType.Star);
            Grid.SetColumn(ProfilePanel, profileColumn);
            MainGrid.ColumnDefinitions[profileColumn].Width = new GridLength(2.8, GridUnitType.Star);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            m_Settings.Save();
            m_Manager.Save();
            m_ConnectionManager.Disconnect();
            m_Watch.Stop();
            m_LogWindow.Close();
            m_WebServer.Stop();
            Application.Current.Shutdown();
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
            dialog.AddTabItem(new StreamChat.SettingsItem(m_Settings, StreamChatPanel, this));
            m_ConnectionManager.FillSettings(dialog);
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
            m_ConnectionManager.Update(deltaTime);
            m_Manager.Update(deltaTime);
            m_Watch.Restart();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CanalManager.Emit(StreamGlassCanals.STREAM_START);
        }
    }
}
