using CorpseLib.Ini;
using CorpseLib.Json;
using CorpseLib.Translation;
using CorpseLib.Web.API.Event;
using StreamGlass.API;
using StreamGlass.API.Event;
using StreamGlass.API.Message;
using StreamGlass.API.Overlay;
using StreamGlass.API.Timer;
using StreamGlass.Audio;
using StreamGlass.Core;
using StreamGlass.Core.Connections;
using StreamGlass.Core.Controls;
using StreamGlass.Core.Profile;
using StreamGlass.Core.Stat;
using StreamGlass.StreamAlert;
using StreamGlass.StreamChat;
using StreamGlass.Twitch;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

namespace StreamGlass
{
    public partial class StreamGlassWindow : Core.Controls.Window
    {
        private readonly StreamGlassSplashScreen m_SplashScreen;
        private readonly List<APlugin> m_Plugins = [];
        private readonly CorpseLib.Web.API.API m_API = new(15007);
        private readonly Stopwatch m_Watch = new();
        private readonly IniFile m_Settings = new();
        private readonly ProfileManager m_ProfileManager;
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
                { "test_button", "Test" },
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
                { "statistic_editor_content", "Content:" },
                { "sound_editor_audio_file", "File:" },
                { "sound_editor_audio_output", "Output:" },
            };
            Translator.AddTranslation(translation);
            Translator.LoadDirectory("./locals");
            Translator.CurrentLanguageChanged += () => m_Settings.GetOrAdd("settings").Set("language", Translator.CurrentLanguage.Name);
        }

        private void InitializeAlertSetting(AlertScrollPanel.AlertType alertType, bool hasGift, Sound? audio, string imgPath, string prefix, string? chatMessage)
        {
            IniSection alertSection = m_Settings.GetOrAdd("alert");
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
            InitializeAlertSetting(AlertScrollPanel.AlertType.INCOMMING_RAID, false, null, "../Assets/parachute.png", "${e.From.DisplayName} is raiding you with ${e.NbViewers} viewers", null);
            InitializeAlertSetting(AlertScrollPanel.AlertType.OUTGOING_RAID, false, null, "../Assets/parachute.png", "You are raiding ${e.To.DisplayName} with ${e.NbViewers} viewers", null);
            InitializeAlertSetting(AlertScrollPanel.AlertType.DONATION, false, null, "../Assets/take-my-money.png", "${e.User.DisplayName} as donated ${e.Amount} ${e.Currency}: ", null);
            InitializeAlertSetting(AlertScrollPanel.AlertType.REWARD, false, new("Assets/alert-sound.wav", string.Empty), "../Assets/chest.png", "${e.From.DisplayName} retrieve ${e.Reward}: ${e.Input}", null);
            InitializeAlertSetting(AlertScrollPanel.AlertType.FOLLOW, false, null, "../Assets/hearts.png", "${e.User.DisplayName} is now following you: ", null);
            InitializeAlertSetting(AlertScrollPanel.AlertType.TIER1, true, null, "../Assets/stars-stack-1.png", "${e.User.DisplayName} as subscribed to you with a tier 1: ", null);
            InitializeAlertSetting(AlertScrollPanel.AlertType.TIER2, true, null, "../Assets/stars-stack-2.png", "${e.User.DisplayName} as subscribed to you with a tier 2: ", null);
            InitializeAlertSetting(AlertScrollPanel.AlertType.TIER3, true, null, "../Assets/stars-stack-3.png", "${e.User.DisplayName} as subscribed to you with a tier 3: ", null);
            InitializeAlertSetting(AlertScrollPanel.AlertType.TIER4, true, null, "../Assets/chess-queen.png", "${e.User.DisplayName} as subscribed to you with a prime: ", null);
            InitializeAlertSetting(AlertScrollPanel.AlertType.SHOUTOUT, false, null, "../Assets/megaphone.png", "${e.Moderator.DisplayName} gave a shoutout to ${e.User.DisplayName}", "Go check ${DisplayName(Lower(e.User.DisplayName))}, who's playing ${Game(e.User.DisplayName)} on https://twitch.tv/${e.User.Name}");
            InitializeAlertSetting(AlertScrollPanel.AlertType.BEING_SHOUTOUT, false, null, "../Assets/megaphone.png", "${e.DisplayName} gave you a shoutout", null);

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

        private static APIWebsocketEndpoint CreateAPIEventEndpoint()
        {
            APIWebsocketEndpoint overlayWebsocketEndpoint = new();
            overlayWebsocketEndpoint.RegisterCanal("chat_message", StreamGlassCanals.CHAT_MESSAGE);
            overlayWebsocketEndpoint.RegisterCanal("chat_connected", StreamGlassCanals.CHAT_CONNECTED);
            overlayWebsocketEndpoint.RegisterCanal("chat_joined", StreamGlassCanals.CHAT_JOINED);
            overlayWebsocketEndpoint.RegisterCanal("user_joined", StreamGlassCanals.USER_JOINED);
            overlayWebsocketEndpoint.RegisterCanal("update_stream_info", StreamGlassCanals.UPDATE_STREAM_INFO);
            overlayWebsocketEndpoint.RegisterCanal("stream_start", StreamGlassCanals.STREAM_START);
            overlayWebsocketEndpoint.RegisterCanal("stream_stop", StreamGlassCanals.STREAM_STOP);
            overlayWebsocketEndpoint.RegisterCanal("donation", StreamGlassCanals.DONATION);
            overlayWebsocketEndpoint.RegisterCanal("follow", StreamGlassCanals.FOLLOW);
            overlayWebsocketEndpoint.RegisterCanal("gift_follow", StreamGlassCanals.GIFT_FOLLOW);
            overlayWebsocketEndpoint.RegisterCanal("raid", StreamGlassCanals.RAID);
            overlayWebsocketEndpoint.RegisterCanal("reward", StreamGlassCanals.REWARD);
            overlayWebsocketEndpoint.RegisterCanal("commands", StreamGlassCanals.COMMANDS);
            overlayWebsocketEndpoint.RegisterCanal("profile_changed_menu_item", StreamGlassCanals.PROFILE_CHANGED_MENU_ITEM);
            overlayWebsocketEndpoint.RegisterCanal("ban", StreamGlassCanals.BAN);
            overlayWebsocketEndpoint.RegisterCanal("held_message", StreamGlassCanals.HELD_MESSAGE);
            overlayWebsocketEndpoint.RegisterCanal("allow_message", StreamGlassCanals.ALLOW_MESSAGE);
            overlayWebsocketEndpoint.RegisterCanal("held_message_moderated", StreamGlassCanals.HELD_MESSAGE_MODERATED);
            overlayWebsocketEndpoint.RegisterCanal("chat_clear", StreamGlassCanals.CHAT_CLEAR);
            overlayWebsocketEndpoint.RegisterCanal("chat_clear_user", StreamGlassCanals.CHAT_CLEAR_USER);
            overlayWebsocketEndpoint.RegisterCanal("chat_clear_message", StreamGlassCanals.CHAT_CLEAR_MESSAGE);
            overlayWebsocketEndpoint.RegisterCanal("shoutout", StreamGlassCanals.SHOUTOUT);
            overlayWebsocketEndpoint.RegisterCanal("being_shoutout", StreamGlassCanals.BEING_SHOUTOUT);
            overlayWebsocketEndpoint.RegisterCanal("start_ads", StreamGlassCanals.START_ADS);
            return overlayWebsocketEndpoint;
        }

        private void InitAPI()
        {
            APIWebsocketEndpoint overlayWebsocketEndpoint = CreateAPIEventEndpoint();
            m_API.AddEndpoint(new EventRegisterEndpoint("/event/register", overlayWebsocketEndpoint));
            m_API.AddEndpoint(new EventUnregisterEndpoint("/event/unregister", overlayWebsocketEndpoint));
            m_API.AddEndpoint(overlayWebsocketEndpoint);
            m_API.AddEndpoint(new EventHTTPEndpoint(overlayWebsocketEndpoint));
            m_API.AddEndpoint(new TimerEndpoint());
            m_API.AddEndpoint(new OverlayHTTPEndpoint());
            m_API.AddEndpoint(new AllMessageEndpoint());
            m_API.AddEndpoint(new ClearMessageEndpoint());

            foreach (APlugin plugin in m_Plugins)
                plugin.RegisterPluginToAPI(m_API);
        }

        private void LoadPlugin(APlugin plugin)
        {
            plugin.OnInit(m_ProfileManager, m_ConnectionManager, m_Settings.GetOrAdd(plugin.Name));
            plugin.RegisterPlugin();
            m_Plugins.Add(plugin);
        }

        private void LoadPlugins()
        {
            if (Directory.Exists("plugins/"))
            {
                string[] files = Directory.GetFiles("plugins/");
                foreach (string file in files)
                {
                    try
                    {
                        Assembly dllAssembly = Assembly.LoadFile(file);
                        foreach (Type type in dllAssembly.GetExportedTypes())
                        {
                            if (type.IsAssignableTo(typeof(APlugin)))
                            {
                                object? pluginInstance = Activator.CreateInstance(type);
                                if (pluginInstance != null && pluginInstance is APlugin plugin)
                                    LoadPlugin(plugin);
                            }
                        }
                    } catch { }
                }
            }
        }

        public StreamGlassWindow(StreamGlassSplashScreen splashScreen) : base(new())
        {
            m_SplashScreen = splashScreen;
            StreamGlassContext.Init();
            StreamGlassContext.LOGGER.Start();

            InitializeComponent();
            m_SplashScreen.UpdateProgressBar(10);
            LoadIni();
            InitializeTranslation();
            m_SplashScreen.UpdateProgressBar(40);
            InitializeBrushPalette();
            m_SplashScreen.UpdateProgressBar(50);
            InitializeSettings();
            m_SplashScreen.UpdateProgressBar(60);

            m_ProfileManager = new(m_ConnectionManager);
            m_ProfileManager.Load();
            UpdateProfilesMenuList();
            m_SplashScreen.UpdateProgressBar(70);

            LoadPlugins();
            LoadPlugin(new TwitchPlugin());
            m_SplashScreen.UpdateProgressBar(80);

            InitAPI();
            m_API.Start();
            m_SplashScreen.UpdateProgressBar(90);

            m_DispatcherTimer.Tick += StreamGlassForm_Tick;
            m_DispatcherTimer.Interval = TimeSpan.FromMilliseconds(50);
            m_Watch.Start();
            m_DispatcherTimer.Start();

            StreamChatPanel.SetConnectionManager(m_ConnectionManager);
            StreamAlertPanel.Init(m_ConnectionManager);
            m_SplashScreen.UpdateProgressBar(100);
        }

        private void UpdateProfilesMenuList()
        {
            ProfilesMenu.Items.Clear();
            ProfilesMenu.Items.Add(StatisticsMenuEdit);
            ProfilesMenu.Items.Add(ProfileMenuEdit);
            ProfilesMenu.Items.Add(ProfilesMenuSeparator);

            StreamGlassCanals.PROFILE_CHANGED_MENU_ITEM.Clear();
            var profiles = m_ProfileManager.Objects;
            foreach (var profile in profiles)
            {
                if (profile.IsSelectable)
                {
                    ProfileMenuItem item = new(m_ProfileManager, profile.ID) { Header = profile.Name };
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
            m_ProfileManager.Save();
            m_ConnectionManager.Disconnect();
            m_Watch.Stop();
            m_DispatcherTimer.Stop();
            m_API.Stop();
            Application.Current.Shutdown();
            StreamGlassContext.Delete();
        }

        private void SettingsToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Core.Settings.Dialog dialog = new(this);
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
            m_ProfileManager.Update(deltaTime);
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
            ProfilesDialog dialog = new(this, m_ProfileManager, m_ConnectionManager);
            dialog.ShowDialog();
            UpdateProfilesMenuList();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            StreamGlassContext.LOGGER.Log("Testing");
            m_ConnectionManager.Test();
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
