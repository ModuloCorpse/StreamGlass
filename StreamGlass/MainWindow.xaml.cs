using CorpseLib.Ini;
using CorpseLib.Translation;
using StreamGlass.Core;
using StreamGlass.Core.Audio;
using StreamGlass.Core.Controls;
using StreamGlass.Core.Profile;
using StreamGlass.Core.Stat;
using StreamGlass.StreamChat;
using StreamGlass.Twitch.Alert;
using StreamGlass.Twitch.StreamChat;
using System;
using System.Globalization;
using System.Windows;

namespace StreamGlass
{
    public partial class MainWindow : Core.Controls.Window
    {
        private readonly SplashScreen m_SplashScreen;
        private readonly Manager m_Manager;
        private readonly RadioGroup<string> m_MenuItemsRadioGroup = new();

        private static void AddDefaultPalette(ref BrushPalette palette, string background, string secondaryBackground, string foreground, string overBack, string dialogBorder)
        {
            palette.AddHexColor("top_bar_background", background);
            palette.AddHexColor("background", background);
            palette.AddHexColor("background_2", secondaryBackground);
            palette.AddHexColor("nud_background", secondaryBackground);
            palette.AddHexColor("chat_background", background);
            palette.AddHexColor("text", foreground);
            palette.AddHexColor("nud_text", foreground);
            palette.AddHexColor("top_bar_text", foreground);
            palette.AddHexColor("chat_message", foreground);
            palette.AddHexColor("top_bar_button_close", "#FF605C");
            palette.AddHexColor("top_bar_button_close_over", overBack);
            palette.AddHexColor("top_bar_button_maximize", "#00CA4E");
            palette.AddHexColor("top_bar_button_maximize_over", overBack);
            palette.AddHexColor("top_bar_button_minimize", "#FFBD44");
            palette.AddHexColor("top_bar_button_minimize_over", overBack);
            palette.AddHexColor("list_background", background);
            palette.AddHexColor("list_item_background", secondaryBackground);
            palette.AddHexColor("list_item_text", foreground);
            palette.AddHexColor("chat_scrollbar", secondaryBackground);
            palette.AddHexColor("dialog_border", dialogBorder);
        }

        private void InitializeBrushPalette()
        {
            BrushPaletteManager palette = GetBrushPalette();
            BrushPalette darkMode = palette.NewDefaultPalette("dark-mode", "Dark Mode", BrushPalette.Type.DARK);
            AddDefaultPalette(ref darkMode, "#18181B", "#505050", "#FFFFFF", "#FFFFFF", "#000000");
            darkMode.AddHexColor("chat_highlight_background", "#FFFFFF");
            darkMode.AddHexColor("chat_highlight_message", "#000000");
            BrushPalette lightMode = palette.NewDefaultPalette("light-mode", "Light Mode", BrushPalette.Type.LIGHT);
            AddDefaultPalette(ref lightMode, "#FFFFFF", "#E5E5E5", "#000000", "#000000", "#000000");
            lightMode.AddHexColor("chat_highlight_background", "#18181B");
            lightMode.AddHexColor("chat_highlight_message", "#FFFFFF");
            palette.SetCurrentPalette("dark-mode");
            palette.Load();
            StreamChatPanel.SetBrushPalette(palette);
            StreamAlertPanel.SetBrushPalette(palette);
        }

        private void InitializeAlertSetting(IniFile settings, AlertScrollPanel.AlertType alertType, bool hasGift, Sound? audio, string imgPath, string prefix, string? chatMessage)
        {
            IniSection alertSection = settings.GetOrAdd("alert");
            string audioFilePath = alertSection.GetOrAdd(string.Format("{0}_audio_file", alertType), audio?.File ?? string.Empty);
            string audioOutputPath = alertSection.GetOrAdd(string.Format("{0}_audio_output", alertType), audio?.Output ?? string.Empty);
            string loadedImgPath = alertSection.GetOrAdd(string.Format("{0}_path", alertType), imgPath);
            string loadedPrefix = alertSection.GetOrAdd(string.Format("{0}_prefix", alertType), prefix);
            string loadedChatMessage = alertSection.GetOrAdd(string.Format("{0}_chat_message", alertType), chatMessage ?? string.Empty);
            bool loadedIsEnabled = alertSection.GetOrAdd(string.Format("{0}_enabled", alertType), "true") == "true";
            bool loadedChatMessageIsEnabled = alertSection.GetOrAdd(string.Format("{0}_chat_message_enabled", alertType), (chatMessage != null) ? "true" : "false") == "true";
            StreamAlertPanel.SetAlertInfo(alertType, new(audioFilePath, audioOutputPath), loadedImgPath, loadedPrefix, loadedChatMessage, loadedIsEnabled, loadedChatMessageIsEnabled);

            if (hasGift)
            {
                string giftAudioFilePath = alertSection.GetOrAdd(string.Format("{0}_gift_audio_file", alertType), audio?.File ?? string.Empty);
                string giftAudioOutputPath = alertSection.GetOrAdd(string.Format("{0}_gift_audio_output", alertType), audio?.Output ?? string.Empty);
                string giftLoadedImgPath = alertSection.GetOrAdd(string.Format("{0}_gift_path", alertType), imgPath);
                string giftLoadedPrefix = alertSection.GetOrAdd(string.Format("{0}_gift_prefix", alertType), prefix);
                string giftLoadedChatMessage = alertSection.GetOrAdd(string.Format("{0}_gift_chat_message", alertType), chatMessage ?? string.Empty);
                bool giftLoadedIsEnabled = alertSection.GetOrAdd(string.Format("{0}_gift_enabled", alertType), "true") == "true";
                bool giftLoadedChatMessageIsEnabled = alertSection.GetOrAdd(string.Format("{0}_gift_chat_message_enabled", alertType), (chatMessage != null) ? "true" : "false") == "true";
                StreamAlertPanel.SetGiftAlertInfo(alertType, new(giftAudioFilePath, giftAudioOutputPath), giftLoadedImgPath, giftLoadedPrefix, giftLoadedChatMessage, giftLoadedIsEnabled, giftLoadedChatMessageIsEnabled);
            }
        }

        internal void InitializeSettings(IniFile settings)
        {
            //TODO Move to Twitch plugin
            //Chat
            IniSection chatSection = settings.GetOrAdd("chat");
            StreamChatPanel.SetDisplayType((ScrollPanelDisplayType)int.Parse(chatSection.GetOrAdd("display_type", "0")));
            StreamChatPanel.SetContentFontSize(double.Parse(chatSection.GetOrAdd("message_font_size", "14")));
            chatSection.Add("do_welcome", "true");
            chatSection.Add("welcome_message", "Hello World! Je suis un bot connecté via StreamGlass!");

            //TODO Move to Twitch plugin
            //Alert
            IniSection alertSection = settings.GetOrAdd("alert");
            StreamAlertPanel.SetDisplayType((ScrollPanelDisplayType)int.Parse(alertSection.GetOrAdd("display_type", "0")));
            StreamAlertPanel.SetContentFontSize(double.Parse(alertSection.GetOrAdd("message_font_size", "20")));
            InitializeAlertSetting(settings, AlertScrollPanel.AlertType.INCOMMING_RAID, false, null, "../Assets/parachute.png", "${e.From.DisplayName} is raiding you with ${e.NbViewers} viewers", null);
            InitializeAlertSetting(settings, AlertScrollPanel.AlertType.OUTGOING_RAID, false, null, "../Assets/parachute.png", "You are raiding ${e.To.DisplayName} with ${e.NbViewers} viewers", null);
            InitializeAlertSetting(settings, AlertScrollPanel.AlertType.DONATION, false, null, "../Assets/take-my-money.png", "${e.User.DisplayName} as donated ${e.Amount} ${e.Currency}: ", null);
            InitializeAlertSetting(settings, AlertScrollPanel.AlertType.REWARD, false, new("Assets/alert-sound.wav", string.Empty), "../Assets/chest.png", "${e.From.DisplayName} retrieve ${e.Reward}: ${e.Input}", null);
            InitializeAlertSetting(settings, AlertScrollPanel.AlertType.FOLLOW, false, null, "../Assets/hearts.png", "${e.User.DisplayName} is now following you: ", null);
            InitializeAlertSetting(settings, AlertScrollPanel.AlertType.TIER1, true, null, "../Assets/stars-stack-1.png", "${e.User.DisplayName} as subscribed to you with a tier 1: ", null);
            InitializeAlertSetting(settings, AlertScrollPanel.AlertType.TIER2, true, null, "../Assets/stars-stack-2.png", "${e.User.DisplayName} as subscribed to you with a tier 2: ", null);
            InitializeAlertSetting(settings, AlertScrollPanel.AlertType.TIER3, true, null, "../Assets/stars-stack-3.png", "${e.User.DisplayName} as subscribed to you with a tier 3: ", null);
            InitializeAlertSetting(settings, AlertScrollPanel.AlertType.TIER4, true, null, "../Assets/chess-queen.png", "${e.User.DisplayName} as subscribed to you with a prime: ", null);
            InitializeAlertSetting(settings, AlertScrollPanel.AlertType.SHOUTOUT, false, null, "../Assets/megaphone.png", "${e.Moderator.DisplayName} gave a shoutout to ${e.User.DisplayName}", "Go check ${DisplayName(Lower(e.User.DisplayName))}, who's playing ${Game(e.User.DisplayName)} on https://twitch.tv/${e.User.Name}");
            InitializeAlertSetting(settings, AlertScrollPanel.AlertType.BEING_SHOUTOUT, false, null, "../Assets/megaphone.png", "${e.DisplayName} gave you a shoutout", null);

            //TODO Move to Twitch plugin
            //Held message
            IniSection moderationSection = settings.GetOrAdd("moderation");
            HeldMessagePanel.SetDisplayType((ScrollPanelDisplayType)int.Parse(moderationSection.GetOrAdd("display_type", "0")));
            HeldMessagePanel.SetSenderFontSize(double.Parse(moderationSection.GetOrAdd("sender_font_size", "14")));
            HeldMessagePanel.SetSenderWidth(double.Parse(moderationSection.GetOrAdd("sender_size", "200")));
            HeldMessagePanel.SetContentFontSize(double.Parse(moderationSection.GetOrAdd("message_font_size", "14")));

            //Settings
            IniSection settingsSection = settings.GetOrAdd("settings");
            Translator.SetLanguage(CultureInfo.GetCultureInfo(settingsSection.GetOrAdd("language", "en-US")));
        }

        public MainWindow(SplashScreen splashScreen) : base(new())
        {
            m_SplashScreen = splashScreen;
            StreamGlassContext.Init();
            StreamGlassContext.LOGGER.Start();
            StreamGlassCanals.Register<string>("profile_changed_menu_item", (profileID) => m_MenuItemsRadioGroup.Select(profileID!));

            InitializeComponent();
            m_SplashScreen.UpdateProgressBar(10);
            InitializeBrushPalette();
            m_SplashScreen.UpdateProgressBar(30);
            m_Manager = new(splashScreen, this);
            UpdateProfilesMenuList();

            HeldMessagePanel.Init();
            StreamChatPanel.Init();
            StreamAlertPanel.Init(m_Manager.ConnectionManager);
            m_SplashScreen.UpdateProgressBar(100);
        }

        internal void UpdateProfilesMenuList()
        {
            ProfilesMenu.Items.Clear();
            ProfilesMenu.Items.Add(StatisticsMenuEdit);
            ProfilesMenu.Items.Add(ProfileMenuEdit);
            ProfilesMenu.Items.Add(ProfilesMenuSeparator);

            m_MenuItemsRadioGroup.Clear();
            var profiles = m_Manager.ProfileManager.Objects;
            foreach (var profile in profiles)
            {
                if (profile.IsSelectable)
                {
                    ProfileMenuItem item = new(m_Manager.ProfileManager, profile.ID) { Header = profile.Name };
                    ProfilesMenu.Items.Add(item);
                    m_MenuItemsRadioGroup.AddItem(item);
                }
            }
            ProfilesMenu.Update(GetBrushPalette());
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            GetBrushPalette().Save();
            Translator.SaveToDir("./locals");
            m_Manager.Stop();
            Application.Current.Shutdown();
            StreamGlassContext.Delete();
        }

        private void SettingsToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Core.Settings.Dialog dialog = new(this);
            dialog.AddTabItem(new GeneralSettingsItem(m_Manager.GetOrAddSettings("stream-chat"), GetBrushPalette()));
            dialog.AddTabItem(new StreamChatSettingsItem(m_Manager.GetOrAddSettings("chat"), StreamChatPanel));
            dialog.AddTabItem(new StreamAlertSettingsItem(m_Manager.GetOrAddSettings("alert"), StreamAlertPanel));
            m_Manager.FillSettingsDialog(dialog);
            dialog.ShowDialog();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void MaximizeRestoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                if (WindowState == WindowState.Maximized)
                    SystemCommands.RestoreWindow(this);
                else
                    SystemCommands.MaximizeWindow(this);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        private void EditStatisticsButton_Click(object sender, RoutedEventArgs e)
        {
            StatisticFileDialog dialog = new(this);
            dialog.ShowDialog();
        }

        private void EditProfilesButton_Click(object sender, RoutedEventArgs e)
        {
            ProfilesDialog dialog = new(this, m_Manager.ProfileManager, m_Manager.ConnectionManager);
            dialog.ShowDialog();
            UpdateProfilesMenuList();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            StreamGlassContext.LOGGER.Log("Testing");
            m_Manager.Test();
            StreamGlassContext.LOGGER.Log("Testing done");
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            Top = Properties.Settings.Default.Top;
            Left = Properties.Settings.Default.Left;
            Height = Properties.Settings.Default.Height;
            Width = Properties.Settings.Default.Width;
            if (Properties.Settings.Default.Maximized)
                WindowState = WindowState.Maximized;
            m_SplashScreen.Close();
            Activate();
            Focus();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                Properties.Settings.Default.Top = RestoreBounds.Top;
                Properties.Settings.Default.Left = RestoreBounds.Left;
                Properties.Settings.Default.Height = RestoreBounds.Height;
                Properties.Settings.Default.Width = RestoreBounds.Width;
                Properties.Settings.Default.Maximized = true;
            }
            else
            {
                Properties.Settings.Default.Top = Top;
                Properties.Settings.Default.Left = Left;
                Properties.Settings.Default.Height = Height;
                Properties.Settings.Default.Width = Width;
                Properties.Settings.Default.Maximized = false;
            }

            Properties.Settings.Default.Save();
        }
    }
}
