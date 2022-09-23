using Quicksand.Web;
using StreamGlass.Command;
using StreamGlass.Twitch.IRC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StreamGlass
{
    /// <summary>
    /// Interaction logic for StreamGlassWindow.xaml
    /// </summary>
    public partial class StreamGlassWindow : Window
    {
        private readonly BrushPaletteManager m_ChatPalette = new();
        private readonly Stopwatch m_Watch = new();
        private readonly Settings m_Settings = new();
        private readonly Server m_WebServer = new();
        private readonly ProfileManager m_Manager = new();
        private readonly TwitchBot m_Bot;

        public StreamGlassWindow()
        {
            Profile.Init();
            m_ChatPalette.Load();
            m_Settings.Load();
            m_Manager.Load();
            InitializeComponent();
            StreamChatPanel.InitBrushPalette(m_ChatPalette);
            UpdateColorPalette();
            m_WebServer.GetResourceManager().AddFramework();
            m_WebServer.Start();

            m_Bot = new(m_WebServer, m_Settings, m_Manager, this);

            m_ChatPalette.FillComboBox(ref ChatModeComboBox);
            m_Manager.FillComboBox(ref CommandProfilesComboBox);

            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += StreamGlassForm_Tick;
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(50);
            m_Watch.Start();
            dispatcherTimer.Start();

            if (m_Settings.Get("twitch", "auto_connect") == "true")
                m_Bot.Connect();
        }

        private void UpdateColorPalette()
        {
            ChatPanelHeader.Background = m_ChatPalette.GetColor("background");
            StreamChatPanel.UpdateColorPalette();
        }

        private void CommandProfilesComboBox_SelectionChanged(object sender, EventArgs e)
        {
            Profile.Info selector = (Profile.Info)CommandProfilesComboBox.SelectedItem;
            m_Manager.SetCurrentProfile(selector.ID);
        }

        private void ChatModeComboBox_SelectionChanged(object sender, EventArgs e)
        {
            BrushPalette.Info selector = (BrushPalette.Info)ChatModeComboBox.SelectedItem;
            m_ChatPalette.SetCurrentPalette(selector.ID);
            UpdateColorPalette();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            m_Settings.Save();
            m_Manager.Save();
            m_ChatPalette.Save();
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
