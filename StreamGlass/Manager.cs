using CorpseLib.Ini;
using CorpseLib.Json;
using CorpseLib.Translation;
using CorpseLib.Web.API.Event;
using StreamGlass.API;
using StreamGlass.API.Event;
using StreamGlass.API.Overlay;
using StreamGlass.Core;
using StreamGlass.Core.Plugin;
using StreamGlass.Core.Profile;
using StreamGlass.Twitch;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Threading;

namespace StreamGlass
{
    public class Manager
    {
        private readonly CorpseLib.Web.API.API m_API = new(15007);
        private readonly Stopwatch m_Watch = new();
        private readonly DispatcherTimer m_DispatcherTimer = new();
        private readonly IniFile m_Settings = new();
        private readonly ProfileManager m_ProfileManager;
        private readonly PluginManager m_PluginManager;

        public ProfileManager ProfileManager => m_ProfileManager;

        //TODO Temporary code : To remove
        private readonly TwitchPlugin m_TwitchPlugin = new();
        public TwitchPlugin TwitchPlugin => m_TwitchPlugin;
        //

        public Manager(SplashScreen splashScreen)
        {
            LoadIni();
            InitializeTranslation();
            splashScreen.UpdateProgressBar(50);

            m_ProfileManager = new();
            m_ProfileManager.Load();
            splashScreen.UpdateProgressBar(60);

            m_PluginManager = new();
            m_PluginManager.LoadPlugins();
            //TODO Temporary code : To replace with "new TwitchPlugin()"
            m_PluginManager.LoadPlugin(m_TwitchPlugin);
            splashScreen.UpdateProgressBar(70);

            m_ProfileManager.UpdateStreamInfo();

            InitAPI();
            m_API.Start();
            splashScreen.UpdateProgressBar(80);

            //Last initalization to do
            m_PluginManager.InitPlugins();
            m_DispatcherTimer.Tick += Tick;
            m_DispatcherTimer.Interval = TimeSpan.FromMilliseconds(50);
            m_Watch.Start();
            m_DispatcherTimer.Start();
            splashScreen.UpdateProgressBar(90);
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

        private void InitAPI()
        {
            APIWebsocketEndpoint overlayWebsocketEndpoint = new();
            foreach (StreamGlassCanals.ACanalManager manager in StreamGlassCanals.Managers)
                overlayWebsocketEndpoint.RegisterJCanal(manager.Type, manager.JCanal);
            StreamGlassCanals.OnManagerAdded += (StreamGlassCanals.ACanalManager manager) => overlayWebsocketEndpoint.RegisterJCanal(manager.Type, manager.JCanal);
            StreamGlassCanals.OnManagerRemoved += (StreamGlassCanals.ACanalManager manager) => overlayWebsocketEndpoint.UnregisterJCanal(manager.Type, manager.JCanal);
            m_API.AddEndpoint(new EventRegisterEndpoint("/event/register", overlayWebsocketEndpoint));
            m_API.AddEndpoint(new EventUnregisterEndpoint("/event/unregister", overlayWebsocketEndpoint));
            m_API.AddEndpoint(overlayWebsocketEndpoint);
            m_API.AddEndpoint(new EventHTTPEndpoint(overlayWebsocketEndpoint));
            m_API.AddEndpoint(new OverlayHTTPEndpoint());

            m_PluginManager.RegisterToAPI(m_API);
        }

        private void Tick(object? sender, EventArgs e)
        {
            m_Watch.Stop();
            long deltaTime = m_Watch.ElapsedMilliseconds;
            m_PluginManager.Update(deltaTime);
            m_ProfileManager.Update(deltaTime);
            m_Watch.Restart();
        }

        public void Stop()
        {
            m_Settings.WriteToFile("settings.ini");
            m_ProfileManager.Save();
            m_PluginManager.Clear();
            m_Watch.Stop();
            m_DispatcherTimer.Stop();
            m_API.Stop();
        }

        public IniSection GetOrAddSettings(string sectionName) => m_Settings.GetOrAdd(sectionName);

        public void FillSettingsDialog(Core.Settings.Dialog settingsDialog) => m_PluginManager.FillSettings(settingsDialog);

        public void Test() => m_PluginManager.Test();
    }
}
