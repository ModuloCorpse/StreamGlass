using Quicksand.Web;
using StreamGlass.Profile;
using StreamGlass.StreamAlert;
using StreamGlass.StreamChat;
using StreamGlass.Controls;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Threading;
using StreamGlass.Connections;
using CorpseLib.Translation;

namespace StreamGlass
{
    public partial class StreamGlassWindow : Controls.Window
    {
        private readonly Stopwatch m_Watch = new();
        private readonly Settings.Data m_Settings = new();
        private readonly Server m_WebServer = new();
        private readonly ProfileManager m_Manager;
        private readonly ConnectionManager m_ConnectionManager = new();
        private readonly DispatcherTimer m_DispatcherTimer = new();
        private bool m_ChatPanelOnRight = false;

        private static void AddDefaultPalette(ref BrushPalette palette,
            string background,
            string secondaryBackground,
            string foreground,
            string overBack,
            string dialogBorder)
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

        private void InitializeTranslation()
        {
            Translation translation = new(new CultureInfo("en-US"), true)
            {
                { "menu_file", "File" },
                { "menu_settings", "Settings" },
                { "menu_profile", "Profiles" },
                { "menu_edit_profile", "Edit profiles..." },
                { "menu_help", "Help" },
                { "menu_logs", "Logs" },
                { "menu_about", "About" },
                { "app_name", "Stream Glass" },
                { "tab_stream_events", "Events" },
                { "tab_stream_overlay", "Overlay" },
                { "settings_general_color", "Color Theme:" },
                { "settings_general_language", "Language:" },
                { "save_button", "Save" },
                { "close_button", "Close" },
                { "section_profiles", "Profiles" },
                { "section_stream_info", "Stream Info" },
                { "section_commands", "Commands" },
                { "section_aliases", "Aliases" },
                { "section_sub_commands", "Sub Commands" },
                { "section_auto_trigger", "Auto Trigger" },
                { "section_default_arguments", "Default Arguments" },
                { "profile_editor_name", "Name:" },
                { "profile_editor_parent", "Parent:" },
                { "profile_editor_is_selectable", "Is Selectable:" },
                { "profile_editor_title", "Title:" },
                { "profile_editor_category", "Category:" },
                { "profile_editor_description", "Description:" },
                { "profile_editor_language", "Language:" },
                { "profile_editor_content", "Content:" },
                { "profile_editor_time", "Time:" },
                { "profile_editor_messages", "Messages Number:" },
                { "profile_editor_arguments", "Arguments Number:" },
                { "profile_editor_user", "User:" },
                { "profile_editor_enable", "Enable:" },
                { "profile_editor_time_delta", "Time Delta:" },
                { "settings_chat_mode", "Chat Mode:" },
                { "settings_chat_name_font", "Name Font Size:" },
                { "settings_chat_name_width", "Chat Name Width:" },
                { "settings_chat_font", "Chat Font Size:" },
                { "settings_chat_right", "Chat Panel On Right:" },
                { "settings_chat_do_welcome", "Do Welcome:" },
                { "settings_chat_welcome_message", "Welcome Message:" },
                { "settings_twitch_autoconnect", "Auto-Connect:" },
                { "settings_twitch_connect", "Connect" },
                { "settings_twitch_browser", "Browser:" },
                { "settings_twitch_channel", "Twitch channel:" },
                { "settings_twitch_bot_public", "Bot Public Key:" },
                { "settings_twitch_bot_private", "Bot Secret Key:" },
                { "settings_twitch_sub_mode", "Sub Mode:" },
                { "settings_twitch_search", "Search:" },
                { "user_type_none", "Viewer" },
                { "user_type_mod", "Moderator" },
                { "user_type_global_mod", "Platform Moderator" },
                { "user_type_admin", "Platform Administrator" },
                { "user_type_staff", "Platform Staff" },
                { "user_type_broadcaster", "Streamer" },
                { "user_type_self", "Bot" },
                { "chat_display_type_ttb", "To bottom" },
                { "chat_display_type_rttb", "Reversed to bottom" },
                { "chat_display_type_btt", "To top" },
                { "chat_display_type_rbtt", "Reversed to top" },
                { "sub_mode_claimed", "Claimed" },
                { "sub_mode_all", "All" },
                { "alert_editor_enable", "Enable:" },
                { "alert_editor_image", "Image:" },
                { "alert_editor_prefix", "Prefix:" },
                { "settings_alert_alerts", "Alerts" },
                { "alert_name_inc_raid", "Incomming raid" },
                { "alert_name_out_raid", "Outgoing raid" },
                { "alert_name_donation", "Donation" },
                { "alert_name_reward", "Reward" },
                { "alert_name_follow", "Follow" },
                { "alert_name_tier1", "Tier 1" },
                { "alert_name_tier2", "Tier 2" },
                { "alert_name_tier3", "Tier 3" },
                { "alert_name_tier4", "Prime/Tier 4" },
                { "ban_button", "Ban" },
                { "ban_time", "Time (s):" },
                { "ban_reason", "Reason:" },
                { "message_menu_highlight", "Toggle Highlight" },
                { "message_menu_ban", "Ban User" }
            };
            Translator.AddTranslation(translation);
            Translator.LoadDirectory("./locals");
            Translator.CurrentLanguageChanged += () => m_Settings.Set("settings", "language", Translator.CurrentLanguage.Name);
        }

        private void InitializeAlertSetting(AlertScrollPanel.AlertType alertType, string imgPath, string prefix, bool isEnabled)
        {
            m_Settings.Create("alert", string.Format("{0}_path", alertType), imgPath);
            m_Settings.Create("alert", string.Format("{0}_prefix", alertType), prefix);
            m_Settings.Create("alert", string.Format("{0}_enabled", alertType), (isEnabled) ? "true" : "false");
            string loadedImgPath = m_Settings.Get("alert", string.Format("{0}_path", alertType));
            string loadedPrefix = m_Settings.Get("alert", string.Format("{0}_prefix", alertType));
            bool loadedIsEnabled = m_Settings.Get("alert", string.Format("{0}_enabled", alertType)) == "true";
            StreamAlertPanel.SetAlertInfo(alertType, loadedImgPath, loadedPrefix, loadedIsEnabled);

            m_Settings.Create("alert", string.Format("{0}_gift_path", alertType), imgPath);
            m_Settings.Create("alert", string.Format("{0}_gift_prefix", alertType), prefix);
            m_Settings.Create("alert", string.Format("{0}_gift_enabled", alertType), (isEnabled) ? "true" : "false");
            string giftLoadedImgPath = m_Settings.Get("alert", string.Format("{0}_gift_path", alertType));
            string giftLoadedPrefix = m_Settings.Get("alert", string.Format("{0}_gift_prefix", alertType));
            bool giftLoadedIsEnabled = m_Settings.Get("alert", string.Format("{0}_gift_enabled", alertType)) == "true";
            StreamAlertPanel.SetGiftAlertInfo(alertType, giftLoadedImgPath, giftLoadedPrefix, giftLoadedIsEnabled);
        }

        private void InitializeSettings()
        {
            //Chat
            m_Settings.Create("chat", "display_type", "0");
            StreamChatPanel.SetDisplayType((ScrollPanelDisplayType)int.Parse(m_Settings.Get("chat", "display_type")));
            m_Settings.Create("chat", "sender_font_size", "14");
            StreamChatPanel.SetSenderFontSize(double.Parse(m_Settings.Get("chat", "sender_font_size")));
            m_Settings.Create("chat", "sender_size", "100");
            StreamChatPanel.SetSenderWidth(double.Parse(m_Settings.Get("chat", "sender_size")));
            m_Settings.Create("chat", "message_font_size", "14");
            StreamChatPanel.SetContentFontSize(double.Parse(m_Settings.Get("chat", "message_font_size")));
            m_Settings.Create("chat", "panel_on_right", "false");
            if (m_Settings.Get("chat", "panel_on_right") == "true")
                SetChatPanelOnRight();
            m_Settings.Create("chat", "do_welcome", "true");
            m_Settings.Create("chat", "welcome_message", "Hello World! Je suis un bot connecté via StreamGlass!");

            //Alert
            m_Settings.Create("alert", "display_type", "0");
            StreamAlertPanel.SetDisplayType((ScrollPanelDisplayType)int.Parse(m_Settings.Get("alert", "display_type")));
            m_Settings.Create("alert", "message_font_size", "20");
            StreamAlertPanel.SetContentFontSize(double.Parse(m_Settings.Get("alert", "message_font_size")));
            InitializeAlertSetting(AlertScrollPanel.AlertType.INCOMMING_RAID, "../Assets/parachute.png", "${e.From} is raiding you with ${e.NbViewers} viewers", true);
            InitializeAlertSetting(AlertScrollPanel.AlertType.OUTGOING_RAID, "../Assets/parachute.png", "You are raiding ${e.To} with ${e.NbViewers} viewers", true);
            InitializeAlertSetting(AlertScrollPanel.AlertType.DONATION, "../Assets/take-my-money.png", "${e.Name} as donated ${e.Amount} ${e.Currency}: ", true);
            InitializeAlertSetting(AlertScrollPanel.AlertType.REWARD, "../Assets/chest.png", "${e.From} retrieve ${e.Reward}: ${e.Input}", true);
            InitializeAlertSetting(AlertScrollPanel.AlertType.FOLLOW, "../Assets/hearts.png", "${e.Name} is now following you: ", true);
            InitializeAlertSetting(AlertScrollPanel.AlertType.TIER1, "../Assets/stars-stack-1.png", "${e.Name} as subscribed to you with a tier 1: ", true);
            InitializeAlertSetting(AlertScrollPanel.AlertType.TIER2, "../Assets/stars-stack-2.png", "${e.Name} as subscribed to you with a tier 2: ", true);
            InitializeAlertSetting(AlertScrollPanel.AlertType.TIER3, "../Assets/stars-stack-3.png", "${e.Name} as subscribed to you with a tier 3: ", true);
            InitializeAlertSetting(AlertScrollPanel.AlertType.TIER4, "../Assets/chess-queen.png", "${e.Name} as subscribed to you with a prime: ", true);

            //Held message
            m_Settings.Create("moderation", "display_type", "0");
            HeldMessagePanel.SetDisplayType((ScrollPanelDisplayType)int.Parse(m_Settings.Get("moderation", "display_type")));
            m_Settings.Create("moderation", "sender_font_size", "14");
            HeldMessagePanel.SetSenderFontSize(double.Parse(m_Settings.Get("moderation", "sender_font_size")));
            m_Settings.Create("moderation", "sender_size", "200");
            HeldMessagePanel.SetSenderWidth(double.Parse(m_Settings.Get("moderation", "sender_size")));
            m_Settings.Create("moderation", "message_font_size", "14");
            HeldMessagePanel.SetContentFontSize(double.Parse(m_Settings.Get("moderation", "message_font_size")));

            //Settings
            m_Settings.Create("settings", "language", "en-US");
            Translator.SetLanguage(CultureInfo.GetCultureInfo(m_Settings.Get("settings", "language")));
        }

        public StreamGlassWindow(): base(new())
        {
            InitializeComponent();
            m_Settings.Load();
            InitializeSettings();
            InitializeBrushPalette();
            InitializeTranslation();

            m_WebServer.GetResourceManager().AddFramework();
            m_WebServer.Start();

            m_ConnectionManager.RegisterConnection(new Twitch.Connection(m_WebServer, m_Settings, this));
            m_Manager = new(m_ConnectionManager);
            m_Manager.Load();
            UpdateProfilesMenuList();

            m_DispatcherTimer.Tick += StreamGlassForm_Tick;
            m_DispatcherTimer.Interval = TimeSpan.FromMilliseconds(50);
            m_Watch.Start();
            m_DispatcherTimer.Start();

            StreamChatPanel.SetConnectionManager(m_ConnectionManager);
            StreamAlertPanel.SetConnectionManager(m_ConnectionManager);

            //TODO Fix translation still on en-US on bootup
            Update();
        }

        private void UpdateProfilesMenuList()
        {
            ProfilesMenu.Items.Clear();
            ProfilesMenu.Items.Add(ProfileMenuEdit);
            ProfilesMenu.Items.Add(ProfilesMenuSeparator);

            StreamGlassCanals.PROFILE_CHANGED_MENU_ITEM.Clear();
            var profiles = m_Manager.Objects;
            foreach (var profile in profiles)
            {
                if (profile.IsSelectable)
                {
                    ProfileMenuItem item = new(m_Manager, profile.ID) { Header = profile.Name };
                    ProfilesMenu.Items.Add(item);
                }
            }
            ProfilesMenu.Update(GetBrushPalette());
        }

        public bool IsChatPanelOnRight => m_ChatPanelOnRight;

        public void SetChatPanelOnLeft()
        {
            SetColumns(0, 1);
            m_ChatPanelOnRight = false;
        }

        public void SetChatPanelOnRight()
        {
            SetColumns(1, 0);
            m_ChatPanelOnRight = true;
        }

        private void SetColumns(int chatColumn, int profileColumn)
        {
            System.Windows.Controls.Grid.SetColumn(StreamChatDock, chatColumn);
            MainGrid.ColumnDefinitions[chatColumn].Width = new GridLength(2, GridUnitType.Star);
            System.Windows.Controls.Grid.SetColumn(ProfilePanel, profileColumn);
            MainGrid.ColumnDefinitions[profileColumn].Width = new GridLength(3, GridUnitType.Star);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            GetBrushPalette().Save();
            Translator.SaveToDir("./locals");
            m_Settings.Save();
            m_Manager.Save();
            m_ConnectionManager.Disconnect();
            m_Watch.Stop();
            m_DispatcherTimer.Stop();
            m_WebServer.Stop();
            Application.Current.Shutdown();
        }

        internal void JoinChannel(string channel)
        {
            m_Manager.UpdateStreamInfo();
            if (m_Settings.Get("chat", "do_welcome") == "true")
                m_ConnectionManager.SendMessage(channel, m_Settings.Get("chat", "welcome_message"));
        }

        private void SettingsToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Settings.Dialog dialog = new(this);
            dialog.AddTabItem(new GeneralSettingsItem(m_Settings, GetBrushPalette()));
            dialog.AddTabItem(new StreamChatSettingsItem(m_Settings, StreamChatPanel, this));
            dialog.AddTabItem(new StreamAlertSettingsItem(m_Settings, StreamAlertPanel));
            m_ConnectionManager.FillSettings(dialog);
            dialog.ShowDialog();
        }

        private void StreamGlassForm_Tick(object? sender, EventArgs e)
        {
            m_Watch.Stop();
            long deltaTime = m_Watch.ElapsedMilliseconds;
            m_ConnectionManager.Update(deltaTime);
            m_Manager.Update(deltaTime);
            m_Watch.Restart();
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

        private void EditProfilesButton_Click(object sender, RoutedEventArgs e)
        {
            ProfilesDialog dialog = new(this, m_Manager, m_ConnectionManager);
            dialog.ShowDialog();
            UpdateProfilesMenuList();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            m_ConnectionManager.Test();
            StreamGlassCanals.FOLLOW.Emit(new("Jean-Michel Jarre", new("J'aime le Pop-Corn"), 0, 69, 42));
            StreamGlassCanals.FOLLOW.Emit(new("Jean-Michel Jarre", new("J'aime le Pop-Corn"), 1, 69, 42));
            StreamGlassCanals.FOLLOW.Emit(new("Jean-Michel Jarre", new("J'aime le Pop-Corn"), 2, 69, 42));
            StreamGlassCanals.FOLLOW.Emit(new("Jean-Michel Jarre", new("J'aime le Pop-Corn"), 3, 69, 42));
            StreamGlassCanals.FOLLOW.Emit(new("Jean-Michel Jarre", new("J'aime le Pop-Corn"), 4, 69, 42));
            StreamGlassCanals.GIFT_FOLLOW.Emit(new("Capterge", "Jean-Michel Jarre", new("Il aime le Pop-Corn"), 1, 69, 42, -1));
            StreamGlassCanals.DONATION.Emit(new("Jean-Michel Jarre", 666, "bits", new("J'aime le Pop-Corn")));
            StreamGlassCanals.RAID.Emit(new("", "Jean-Michel Jarre", "", "Capterge", 40, true));
            StreamGlassCanals.REWARD.Emit(new("", "Jean-Michel Jarre", "Chante", "J'aime le Pop-Corn"));
        }
    }
}
