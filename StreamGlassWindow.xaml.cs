using Quicksand.Web;
using StreamGlass.Command;
using StreamGlass.Twitch.IRC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace StreamGlass
{
    /// <summary>
    /// Interaction logic for StreamGlassWindow.xaml
    /// </summary>
    public partial class StreamGlassWindow : Window
    {
        private readonly List<UserMessage> m_Messages = new();
        private readonly Stopwatch m_Watch = new();
        private readonly Settings m_Settings = new();
        private readonly Server m_WebServer = new();
        private readonly ProfileManager m_Manager = new();
        private readonly TwitchBot m_Bot;

        public StreamGlassWindow()
        {
            Profile.Init();
            InitializeComponent();
            m_WebServer.GetResourceManager().AddFramework();
            m_WebServer.Start();
            m_Settings.Load();
            m_Manager.Load();

            m_Bot = new(m_WebServer, m_Settings, m_Manager, this);

            UpdateCommandProfilesList();

            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += StreamGlassForm_Tick;
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(50);
            m_Watch.Start();
            dispatcherTimer.Start();
        }

        private class CommandProfileSelector
        {
            private readonly string m_ID;
            private readonly string m_Name;
            public CommandProfileSelector(string id, string name)
            {
                m_ID = id;
                m_Name = name;
            }
            public string ID => m_ID;
            public string Name => m_Name;
            public override string? ToString() => m_Name;
        }

        internal void UpdateCommandProfilesList()
        {
            string currentProfileID = m_Manager.CurrentProfileID;
            List<Profile> profiles = m_Manager.Profiles;
            CommandProfilesComboBox.Items.Clear();
            foreach (Profile profile in profiles)
            {
                CommandProfileSelector selector = new(profile.ID, profile.Name);
                CommandProfilesComboBox.Items.Add(selector);
                if (selector.ID == currentProfileID)
                    CommandProfilesComboBox.SelectedItem = selector;
            }
        }

        private void CommandProfilesComboBox_SelectionChanged(object sender, EventArgs e)
        {
            CommandProfileSelector selector = (CommandProfileSelector)CommandProfilesComboBox.SelectedItem;
            m_Manager.SetCurrentProfile(selector.ID);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            m_Settings.Save();
            m_Manager.Save();
        }

        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SettingsDialog dialog = new(m_Settings, m_Bot);
            dialog.Width = 500;
            dialog.Height = 300;
            dialog.Owner = this;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.Show();
        }

        internal void AddMessage(UserMessage message) => m_Messages.Add(message);

        private void StreamGlassForm_Tick(object? sender, EventArgs e)
        {
            m_Watch.Stop();
            long deltaTime = m_Watch.ElapsedMilliseconds;
            m_Bot.Update(deltaTime);
            m_Manager.Update(deltaTime);
            m_Watch.Restart();

            foreach (UserMessage message in m_Messages)
            {
                /*ChatMessage chatMessage = new(message)
                {
                    Dock = DockStyle.Top,
                    Visible = true
                };
                ChatPanel.Controls.Add(chatMessage);*/
            }
            m_Messages.Clear();
        }
    }
}
