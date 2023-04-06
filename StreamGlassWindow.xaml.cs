using Quicksand.Web;
using StreamGlass.Profile;
using StreamGlass.StreamAlert;
using StreamGlass.StreamChat;
using StreamFeedstock;
using StreamFeedstock.Controls;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Threading;
using StreamGlass.Connections;
using StreamGlass.Events;

namespace StreamGlass
{
    public partial class StreamGlassWindow : StreamFeedstock.Controls.Window
    {
        private readonly Stopwatch m_Watch = new();
        private readonly Settings.Data m_Settings = new();
        private readonly Server m_WebServer = new();
        private readonly ProfileManager m_Manager;
        private readonly ConnectionManager m_ConnectionManager = new();
        private readonly DispatcherTimer m_DispatcherTimer = new();
        private bool m_ChatPanelOnRight = false;

        private static void InitializeCanals()
        {
            CanalManager.NewCanal<UserMessage>(StreamGlassCanals.CHAT_MESSAGE);
            CanalManager.NewCanal(StreamGlassCanals.CHAT_CONNECTED);
            CanalManager.NewCanal<string>(StreamGlassCanals.CHAT_JOINED);
            CanalManager.NewCanal<User>(StreamGlassCanals.USER_JOINED);
            CanalManager.NewCanal<UpdateStreamInfoArgs>(StreamGlassCanals.UPDATE_STREAM_INFO);
            CanalManager.NewCanal(StreamGlassCanals.STREAM_START);
            CanalManager.NewCanal(StreamGlassCanals.STREAM_STOP);
            CanalManager.NewCanal<DonationEventArgs>(StreamGlassCanals.DONATION);
            CanalManager.NewCanal<FollowEventArgs>(StreamGlassCanals.FOLLOW);
            CanalManager.NewCanal<RaidEventArgs>(StreamGlassCanals.RAID);
            CanalManager.NewCanal<RewardEventArgs>(StreamGlassCanals.REWARD);
            CanalManager.NewCanal<CommandEventArgs>(StreamGlassCanals.COMMANDS);
            CanalManager.NewCanal<string>(StreamGlassCanals.PROFILE_CHANGED_MENU_ITEM);
            CanalManager.NewCanal<BanEventArgs>(StreamGlassCanals.BAN);
            CanalManager.NewCanal<UserMessage>(StreamGlassCanals.HELD_MESSAGE);
            CanalManager.NewCanal<string>(StreamGlassCanals.HELD_MESSAGE_MODERATED);
            CanalManager.NewCanal<MessageAllowedEventArgs>(StreamGlassCanals.ALLOW_MESSAGE);
            CanalManager.NewCanal(StreamGlassCanals.CHAT_CLEAR);
            CanalManager.NewCanal<string>(StreamGlassCanals.CHAT_CLEAR_USER);
            CanalManager.NewCanal<string>(StreamGlassCanals.CHAT_CLEAR_MESSAGE);
        }

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
            TranslationManager translationManager = GetTranslations();
            Translation translation = translationManager.NewDefaultTranslation(new CultureInfo("en"));
            translation.AddTranslation("menu_file", "File");
            translation.AddTranslation("menu_settings", "Settings");
            translation.AddTranslation("menu_profile", "Profiles");
            translation.AddTranslation("menu_edit_profile", "Edit profiles...");
            translation.AddTranslation("menu_help", "Help");
            translation.AddTranslation("menu_logs", "Logs");
            translation.AddTranslation("menu_about", "About");
            translation.AddTranslation("app_name", "Stream Glass");
            translation.AddTranslation("tab_stream_events", "Events");
            translation.AddTranslation("tab_stream_overlay", "Overlay");
            translation.AddTranslation("settings_general_color", "Color Theme:");
            translation.AddTranslation("settings_general_language", "Language:");
            translation.AddTranslation("save_button", "Save");
            translation.AddTranslation("close_button", "Close");
            translation.AddTranslation("section_profiles", "Profiles");
            translation.AddTranslation("section_stream_info", "Stream Info");
            translation.AddTranslation("section_commands", "Commands");
            translation.AddTranslation("section_aliases", "Aliases");
            translation.AddTranslation("section_sub_commands", "Sub Commands");
            translation.AddTranslation("section_auto_trigger", "Auto Trigger");
            translation.AddTranslation("section_default_arguments", "Default Arguments");
            translation.AddTranslation("profile_editor_name", "Name:");
            translation.AddTranslation("profile_editor_parent", "Parent:");
            translation.AddTranslation("profile_editor_is_selectable", "Is Selectable:");
            translation.AddTranslation("profile_editor_title", "Title:");
            translation.AddTranslation("profile_editor_category", "Category:");
            translation.AddTranslation("profile_editor_description", "Description:");
            translation.AddTranslation("profile_editor_language", "Language:");
            translation.AddTranslation("profile_editor_content", "Content:");
            translation.AddTranslation("profile_editor_time", "Time:");
            translation.AddTranslation("profile_editor_messages", "Messages Number:");
            translation.AddTranslation("profile_editor_arguments", "Arguments Number:");
            translation.AddTranslation("profile_editor_user", "User:");
            translation.AddTranslation("profile_editor_enable", "Enable:");
            translation.AddTranslation("profile_editor_time_delta", "Time Delta:");
            translation.AddTranslation("settings_chat_mode", "Chat Mode:");
            translation.AddTranslation("settings_chat_name_font", "Name Font Size:");
            translation.AddTranslation("settings_chat_name_width", "Chat Name Width:");
            translation.AddTranslation("settings_chat_font", "Chat Font Size:");
            translation.AddTranslation("settings_chat_right", "Chat Panel On Right:");
            translation.AddTranslation("settings_chat_do_welcome", "Do Welcome:");
            translation.AddTranslation("settings_chat_welcome_message", "Welcome Message:");
            translation.AddTranslation("settings_twitch_autoconnect", "Auto-Connect:");
            translation.AddTranslation("settings_twitch_connect", "Connect");
            translation.AddTranslation("settings_twitch_browser", "Browser:");
            translation.AddTranslation("settings_twitch_channel", "Twitch channel:");
            translation.AddTranslation("settings_twitch_bot_public", "Bot Public Key:");
            translation.AddTranslation("settings_twitch_bot_private", "Bot Secret Key:");
            translation.AddTranslation("settings_twitch_sub_mode", "Sub Mode:");
            translation.AddTranslation("settings_twitch_search", "Search:");
            translation.AddTranslation("user_type_none", "Viewer");
            translation.AddTranslation("user_type_mod", "Moderator");
            translation.AddTranslation("user_type_global_mod", "Platform Moderator");
            translation.AddTranslation("user_type_admin", "Platform Administrator");
            translation.AddTranslation("user_type_staff", "Platform Staff");
            translation.AddTranslation("user_type_broadcaster", "Streamer");
            translation.AddTranslation("user_type_self", "Bot");
            translation.AddTranslation("chat_display_type_ttb", "To bottom");
            translation.AddTranslation("chat_display_type_rttb", "Reversed to bottom");
            translation.AddTranslation("chat_display_type_btt", "To top");
            translation.AddTranslation("chat_display_type_rbtt", "Reversed to top");
            translation.AddTranslation("sub_mode_claimed", "Claimed");
            translation.AddTranslation("sub_mode_all", "All");
            translation.AddTranslation("alert_editor_enable", "Enable:");
            translation.AddTranslation("alert_editor_image", "Image:");
            translation.AddTranslation("alert_editor_prefix", "Prefix:");
            translation.AddTranslation("settings_alert_alerts", "Alerts");
            translation.AddTranslation("alert_name_inc_raid", "Incomming raid");
            translation.AddTranslation("alert_name_out_raid", "Outgoing raid");
            translation.AddTranslation("alert_name_donation", "Donation");
            translation.AddTranslation("alert_name_reward", "Reward");
            translation.AddTranslation("alert_name_follow", "Follow");
            translation.AddTranslation("alert_name_tier1", "Tier 1");
            translation.AddTranslation("alert_name_tier2", "Tier 2");
            translation.AddTranslation("alert_name_tier3", "Tier 3");
            translation.AddTranslation("alert_name_tier4", "Prime/Tier 4");
            translation.AddTranslation("ban_button", "Ban");
            translation.AddTranslation("ban_time", "Time (s):");
            translation.AddTranslation("ban_reason", "Reason:");
            translation.AddTranslation("message_menu_highlight", "Toggle Highlight");
            translation.AddTranslation("message_menu_ban", "Ban User");
            translationManager.Load();
            StreamChatPanel.SetTranslations(translationManager);
            StreamAlertPanel.SetTranslations(translationManager);
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
        }

        public StreamGlassWindow(): base(new(), new())
        {
            InitializeCanals();
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
        }

        private void UpdateProfilesMenuList()
        {
            ProfilesMenu.Items.Clear();
            ProfilesMenu.Items.Add(ProfileMenuEdit);
            ProfilesMenu.Items.Add(ProfilesMenuSeparator);

            CanalManager.Clear(StreamGlassCanals.PROFILE_CHANGED_MENU_ITEM);
            var profiles = m_Manager.Objects;
            foreach (var profile in profiles)
            {
                if (profile.IsSelectable)
                {
                    ProfileMenuItem item = new(m_Manager, profile.ID) { Header = profile.Name };
                    ProfilesMenu.Items.Add(item);
                }
            }
            ProfilesMenu.Update(GetBrushPalette(), GetTranslations());
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
            GetTranslations().Save();
            m_Settings.Save();
            m_Manager.Save();
            m_ConnectionManager.Disconnect();
            m_Watch.Stop();
            m_DispatcherTimer.Stop();
            m_WebServer.Stop();
            Log.Stop();
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
            dialog.AddTabItem(new GeneralSettingsItem(m_Settings, GetBrushPalette(), GetTranslations()));
            dialog.AddTabItem(new StreamChatSettingsItem(m_Settings, StreamChatPanel, this));
            dialog.AddTabItem(new StreamAlertSettingsItem(m_Settings, StreamAlertPanel));
            m_ConnectionManager.FillSettings(dialog);
            dialog.ShowDialog();
        }

        private void LogsToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            LogWindow logWindow = new(this, "Twitch IRC");
            logWindow.ShowDialog();
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
            CanalManager.Emit(StreamGlassCanals.FOLLOW, new FollowEventArgs("Jean-Michel Jarre", new("J'aime le Pop-Corn"), 0, false, 69, 42, -1));
            CanalManager.Emit(StreamGlassCanals.FOLLOW, new FollowEventArgs("Jean-Michel Jarre", new("J'aime le Pop-Corn"), 1, false, 69, 42, -1));
            CanalManager.Emit(StreamGlassCanals.FOLLOW, new FollowEventArgs("Jean-Michel Jarre", new("J'aime le Pop-Corn"), 2, false, 69, 42, -1));
            CanalManager.Emit(StreamGlassCanals.FOLLOW, new FollowEventArgs("Jean-Michel Jarre", new("J'aime le Pop-Corn"), 3, false, 69, 42, -1));
            CanalManager.Emit(StreamGlassCanals.FOLLOW, new FollowEventArgs("Jean-Michel Jarre", new("J'aime le Pop-Corn"), 4, false, 69, 42, -1));
            CanalManager.Emit(StreamGlassCanals.DONATION, new DonationEventArgs("Jean-Michel Jarre", 666, "bits", new("J'aime le Pop-Corn")));
            CanalManager.Emit(StreamGlassCanals.RAID, new RaidEventArgs("", "Jean-Michel Jarre", "", "Capterge", 40, true));
            CanalManager.Emit(StreamGlassCanals.REWARD, new RewardEventArgs("", "Jean-Michel Jarre", "Chante", "J'aime le Pop-Corn"));
        }
    }
}
