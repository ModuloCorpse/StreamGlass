using CorpseLib.Ini;
using CorpseLib.Json;
using CorpseLib.Translation;
using StreamGlass.API;
using StreamGlass.API.Overlay;
using StreamGlass.Connections;
using StreamGlass.Controls;
using StreamGlass.Profile;
using StreamGlass.Stat;
using StreamGlass.StreamAlert;
using StreamGlass.StreamChat;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace StreamGlass
{
    public partial class StreamGlassWindow : Controls.Window
    {
        private readonly CorpseLib.Web.API.API m_API = new(15007);
        private readonly Stopwatch m_Watch = new();
        private readonly IniFile m_Settings = new();
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
                { "menu_edit_statistics", "Edit statistics..." },
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
                { "message_menu_ban", "Ban User" },
                { "section_statistics", "Statistics" },
                { "statistic_editor_path", "Path:" },
                { "statistic_editor_content", "Content:" }
            };
            Translator.AddTranslation(translation);
            Translator.LoadDirectory("./locals");
            Translator.CurrentLanguageChanged += () => m_Settings.GetOrAdd("settings").Set("language", Translator.CurrentLanguage.Name);
        }

        private void InitializeAlertSetting(AlertScrollPanel.AlertType alertType, bool hasGift, string imgPath, string prefix, string chatMessage)
        {
            IniSection alertSection = m_Settings.GetOrAdd("alert");
            string loadedImgPath = alertSection.GetOrAdd(string.Format("{0}_path", alertType), imgPath);
            string loadedPrefix = alertSection.GetOrAdd(string.Format("{0}_prefix", alertType), prefix);
            string loadedChatMessage = alertSection.GetOrAdd(string.Format("{0}_chat_message", alertType), chatMessage);
            bool loadedIsEnabled = alertSection.GetOrAdd(string.Format("{0}_enabled", alertType), "true") == "true";
            bool loadedChatMessageIsEnabled = alertSection.GetOrAdd(string.Format("{0}_chat_message_enabled", alertType), "true") == "true";
            StreamAlertPanel.SetAlertInfo(alertType, loadedImgPath, loadedPrefix, loadedChatMessage, loadedIsEnabled, loadedChatMessageIsEnabled);

            if (hasGift)
            {
                string giftLoadedImgPath = alertSection.GetOrAdd(string.Format("{0}_gift_path", alertType), imgPath);
                string giftLoadedPrefix = alertSection.GetOrAdd(string.Format("{0}_gift_prefix", alertType), prefix);
                string giftLoadedChatMessage = alertSection.GetOrAdd(string.Format("{0}_gift_chat_message", alertType), chatMessage);
                bool giftLoadedIsEnabled = alertSection.GetOrAdd(string.Format("{0}_gift_enabled", alertType), "true") == "true";
                bool giftLoadedChatMessageIsEnabled = alertSection.GetOrAdd(string.Format("{0}_gift_chat_message_enabled", alertType), "true") == "true";
                StreamAlertPanel.SetGiftAlertInfo(alertType, giftLoadedImgPath, giftLoadedPrefix, giftLoadedChatMessage, giftLoadedIsEnabled, giftLoadedChatMessageIsEnabled);
            }
        }

        private void InitializeAlertSetting(AlertScrollPanel.AlertType alertType, bool hasGift, string imgPath, string prefix)
        {
            IniSection alertSection = m_Settings.GetOrAdd("alert");
            string loadedImgPath = alertSection.GetOrAdd(string.Format("{0}_path", alertType), imgPath);
            string loadedPrefix = alertSection.GetOrAdd(string.Format("{0}_prefix", alertType), prefix);
            string loadedChatMessage = alertSection.GetOrAdd(string.Format("{0}_chat_message", alertType), string.Empty);
            bool loadedIsEnabled = alertSection.GetOrAdd(string.Format("{0}_enabled", alertType), "true") == "true";
            bool loadedChatMessageIsEnabled = alertSection.GetOrAdd(string.Format("{0}_chat_message_enabled", alertType), "false") == "true";
            StreamAlertPanel.SetAlertInfo(alertType, loadedImgPath, loadedPrefix, loadedChatMessage, loadedIsEnabled, loadedChatMessageIsEnabled);

            if (hasGift)
            {
                string giftLoadedImgPath = alertSection.GetOrAdd(string.Format("{0}_gift_path", alertType), imgPath);
                string giftLoadedPrefix = alertSection.GetOrAdd(string.Format("{0}_gift_prefix", alertType), prefix);
                string giftLoadedChatMessage = alertSection.GetOrAdd(string.Format("{0}_gift_chat_message", alertType), string.Empty);
                bool giftLoadedIsEnabled = alertSection.GetOrAdd(string.Format("{0}_gift_enabled", alertType), "true") == "true";
                bool giftLoadedChatMessageIsEnabled = alertSection.GetOrAdd(string.Format("{0}_gift_chat_message_enabled", alertType), "false") == "true";
                StreamAlertPanel.SetGiftAlertInfo(alertType, giftLoadedImgPath, giftLoadedPrefix, giftLoadedChatMessage, giftLoadedIsEnabled, giftLoadedChatMessageIsEnabled);
            }
        }

        private void InitializeSettings()
        {
            //Chat
            IniSection chatSection = m_Settings.GetOrAdd("chat");
            StreamChatPanel.SetDisplayType((ScrollPanelDisplayType)int.Parse(chatSection.GetOrAdd("display_type", "0")));
            StreamChatPanel.SetSenderFontSize(double.Parse(chatSection.GetOrAdd("sender_font_size", "14")));
            StreamChatPanel.SetSenderWidth(double.Parse(chatSection.GetOrAdd("sender_size", "100")));
            StreamChatPanel.SetContentFontSize(double.Parse(chatSection.GetOrAdd("message_font_size", "14")));
            if (chatSection.GetOrAdd("panel_on_right", "false") == "true")
                SetChatPanelOnRight();
            chatSection.Add("do_welcome", "true");
            chatSection.Add("welcome_message", "Hello World! Je suis un bot connecté via StreamGlass!");

            //Alert
            IniSection alertSection = m_Settings.GetOrAdd("alert");
            StreamAlertPanel.SetDisplayType((ScrollPanelDisplayType)int.Parse(alertSection.GetOrAdd("display_type", "0")));
            StreamAlertPanel.SetContentFontSize(double.Parse(alertSection.GetOrAdd("message_font_size", "20")));
            InitializeAlertSetting(AlertScrollPanel.AlertType.INCOMMING_RAID, false, "../Assets/parachute.png", "${e.From.DisplayName} is raiding you with ${e.NbViewers} viewers");
            InitializeAlertSetting(AlertScrollPanel.AlertType.OUTGOING_RAID, false, "../Assets/parachute.png", "You are raiding ${e.To.DisplayName} with ${e.NbViewers} viewers");
            InitializeAlertSetting(AlertScrollPanel.AlertType.DONATION, false, "../Assets/take-my-money.png", "${e.User.DisplayName} as donated ${e.Amount} ${e.Currency}: ");
            InitializeAlertSetting(AlertScrollPanel.AlertType.REWARD, false, "../Assets/chest.png", "${e.From.DisplayName} retrieve ${e.Reward}: ${e.Input}");
            InitializeAlertSetting(AlertScrollPanel.AlertType.FOLLOW, false, "../Assets/hearts.png", "${e.User.DisplayName} is now following you: ");
            InitializeAlertSetting(AlertScrollPanel.AlertType.TIER1, true, "../Assets/stars-stack-1.png", "${e.User.DisplayName} as subscribed to you with a tier 1: ");
            InitializeAlertSetting(AlertScrollPanel.AlertType.TIER2, true, "../Assets/stars-stack-2.png", "${e.User.DisplayName} as subscribed to you with a tier 2: ");
            InitializeAlertSetting(AlertScrollPanel.AlertType.TIER3, true, "../Assets/stars-stack-3.png", "${e.User.DisplayName} as subscribed to you with a tier 3: ");
            InitializeAlertSetting(AlertScrollPanel.AlertType.TIER4, true, "../Assets/chess-queen.png", "${e.User.DisplayName} as subscribed to you with a prime: ");
            InitializeAlertSetting(AlertScrollPanel.AlertType.SHOUTOUT, false, "../Assets/megaphone.png", "${e.Moderator.DisplayName} gave a shoutout to ${e.User.DisplayName}", "Go check ${DisplayName(Lower(e.User.DisplayName))}, who's playing ${Game(e.User.DisplayName)} on https://twitch.tv/${e.User.Name}");
            InitializeAlertSetting(AlertScrollPanel.AlertType.BEING_SHOUTOUT, false, "../Assets/megaphone.png", "${e.User.DisplayName} gave you a shoutout");

            //Held message
            IniSection moderationSection = m_Settings.GetOrAdd("moderation");
            HeldMessagePanel.SetDisplayType((ScrollPanelDisplayType)int.Parse(moderationSection.GetOrAdd("display_type", "0")));
            HeldMessagePanel.SetSenderFontSize(double.Parse(moderationSection.GetOrAdd("sender_font_size", "14")));
            HeldMessagePanel.SetSenderWidth(double.Parse(moderationSection.GetOrAdd("sender_size", "200")));
            HeldMessagePanel.SetContentFontSize(double.Parse(moderationSection.GetOrAdd("message_font_size", "14")));

            //Settings
            IniSection settingsSection = m_Settings.GetOrAdd("settings");
            Translator.SetLanguage(CultureInfo.GetCultureInfo(settingsSection.GetOrAdd("language", "en-US")));
        }

        private void LoadIni()
        {
            if (File.Exists("settings.ini"))
            {
                IniFile iniFile = IniFile.ParseFile("settings.ini");
                if (!iniFile.HaveEmptySection)
                    m_Settings.Merge(iniFile);
            }
            else if (File.Exists("settings.json"))
            {
                JFile response = JFile.LoadFromFile("settings.json");
                foreach (var item in response)
                {
                    string key = item.Key;
                    JObject value = item.Value.Cast<JObject>()!;
                    IniSection section = new(key);
                    foreach (var item2 in value)
                        section.Add(item2.Key, item2.Value.Cast<string>()!);
                    m_Settings.AddSection(section);
                }
                File.Delete("settings.json");
            }
        }

        private void InitAPI()
        {
            OverlayWebsocketEndpoint overlayWebsocketEndpoint = StreamGlassCanals.CreateAPIEventEndpoint();
            m_API.AddEndpoint(overlayWebsocketEndpoint);
            m_API.AddEndpoint(new TimerEndpoint());
            m_API.AddEndpoint(new OverlayHTTPEndpoint(overlayWebsocketEndpoint));
            m_API.AddEndpoint(new AllMessageEndpoint());
            m_API.AddEndpoint(new ClearMessageEndpoint());
        }

        public StreamGlassWindow(): base(new())
        {
            StreamGlassContext.Init();

            InitAPI();
            m_API.Start();

            InitializeComponent();
            LoadIni();
            InitializeSettings();
            InitializeBrushPalette();
            InitializeTranslation();

            m_ConnectionManager.RegisterConnection(new Twitch.Connection(m_Settings.GetOrAdd("twitch"), this));
            m_Manager = new(m_ConnectionManager);
            m_Manager.Load();
            UpdateProfilesMenuList();

            m_DispatcherTimer.Tick += StreamGlassForm_Tick;
            m_DispatcherTimer.Interval = TimeSpan.FromMilliseconds(50);
            m_Watch.Start();
            m_DispatcherTimer.Start();

            StreamChatPanel.SetConnectionManager(m_ConnectionManager);
            StreamAlertPanel.Init(m_ConnectionManager);
        }

        private void UpdateProfilesMenuList()
        {
            ProfilesMenu.Items.Clear();
            ProfilesMenu.Items.Add(StatisticsMenuEdit);
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
            m_Settings.WriteToFile("settings.ini");
            m_Manager.Save();
            m_ConnectionManager.Disconnect();
            m_Watch.Stop();
            m_DispatcherTimer.Stop();
            m_API.Stop();
            Application.Current.Shutdown();
            StreamGlassContext.Delete();
        }

        internal void JoinChannel()
        {
            m_Manager.UpdateStreamInfo();
            IniSection? chatSection = m_Settings.Get("chat");
            if (chatSection != null && chatSection.Get("do_welcome") == "true")
                m_ConnectionManager.SendMessage(chatSection.Get("welcome_message"));
        }

        private void SettingsToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Settings.Dialog dialog = new(this);
            dialog.AddTabItem(new GeneralSettingsItem(m_Settings.GetOrAdd("stream-chat"), GetBrushPalette()));
            dialog.AddTabItem(new StreamChatSettingsItem(m_Settings.GetOrAdd("chat"), StreamChatPanel, this));
            dialog.AddTabItem(new StreamAlertSettingsItem(m_Settings.GetOrAdd("alert"), StreamAlertPanel));
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

        private void EditStatisticsButton_Click(object sender, RoutedEventArgs e)
        {
            StatisticFileDialog dialog = new(this);
            dialog.ShowDialog();
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
        }
    }
}
