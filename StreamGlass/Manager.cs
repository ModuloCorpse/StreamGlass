using CorpseLib.Ini;
using CorpseLib.Json;
using CorpseLib.Translation;
using CorpseLib.Web.API.Event;
using StreamGlass.API;
using StreamGlass.API.Event;
using StreamGlass.API.Overlay;
using StreamGlass.Core;
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
        private readonly IniFile m_Settings = [];
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

            PluginManager.PLUGIN_LOGGER.Start();
            m_PluginManager = new();
            m_PluginManager.LoadPlugins();
            //TODO Temporary code : To replace with "new TwitchPlugin()"
            m_PluginManager.LoadPlugin(TwitchPlugin.PluginMetadata, m_TwitchPlugin);
            splashScreen.UpdateProgressBar(70);

            Translator.LoadDirectory("./locals");
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
                IniFile iniFile = IniParser.LoadFromFile("settings.ini");
                if (!iniFile.HaveEmptySection)
                    m_Settings.Merge(iniFile);
            }
            else if (File.Exists("settings.json"))
            {
                JsonObject response = JsonParser.LoadFromFile("settings.json");
                foreach (var item in response)
                {
                    string key = item.Key;
                    JsonObject value = item.Value.Cast<JsonObject>()!;
                    IniSection section = new(key);
                    foreach (var item2 in value)
                        section.Add(item2.Key, item2.Value.Cast<string>()!);
                    m_Settings.Add(section);
                }
                File.Delete("settings.json");
            }
        }

        //TODO Switch from string to TranslationKey in StreamGlass.Core
        //TODO Replace in all XAML use of TranslationKey attribute by the SetTranslationKey to keep track of used key
        private void InitializeTranslation()
        {
            Translation translation = new(new CultureInfo("en-US"), true)
            {
                { StreamGlassTranslationKeys.MENU_FILE, "File" },
                { StreamGlassTranslationKeys.MENU_SETTINGS, "Settings" },
                { StreamGlassTranslationKeys.MENU_PROFILE, "Profiles" },
                { StreamGlassTranslationKeys.MENU_EDIT_STRING_SOURCES, "Edit string sources..." },
                { StreamGlassTranslationKeys.MENU_EDIT_PROFILE, "Edit profiles..." },
                { StreamGlassTranslationKeys.MENU_HELP, "Help" },
                { StreamGlassTranslationKeys.MENU_ABOUT, "About" },
                { StreamGlassTranslationKeys.APP_NAME, "Stream Glass" },
                { StreamGlassTranslationKeys.SETTINGS_GENERAL_COLOR, "Color Theme:" },
                { StreamGlassTranslationKeys.SETTINGS_GENERAL_LANGUAGE, "Language:" },
                { StreamGlassTranslationKeys.SAVE_BUTTON, "Save" },
                { StreamGlassTranslationKeys.CLOSE_BUTTON, "Close" },
                { StreamGlassTranslationKeys.TEST_BUTTON, "Test" },
                { StreamGlassTranslationKeys.SECTION_PROFILES, "Profiles" },
                { StreamGlassTranslationKeys.SECTION_STREAM_INFO, "Stream Info" },
                { StreamGlassTranslationKeys.SECTION_COMMANDS, "Commands" },
                { StreamGlassTranslationKeys.SECTION_ALIASES, "Aliases" },
                { StreamGlassTranslationKeys.SECTION_SUB_COMMANDS, "Sub Commands" },
                { StreamGlassTranslationKeys.SECTION_AUTO_TRIGGER, "Auto Trigger" },
                { StreamGlassTranslationKeys.SECTION_DEFAULT_ARGUMENTS, "Default Arguments" },
                { StreamGlassTranslationKeys.PROFILE_EDITOR_NAME, "Name:" },
                { StreamGlassTranslationKeys.PROFILE_EDITOR_PARENT, "Parent:" },
                { StreamGlassTranslationKeys.PROFILE_EDITOR_IS_SELECTABLE, "Is Selectable:" },
                { StreamGlassTranslationKeys.PROFILE_EDITOR_TITLE, "Title:" },
                { StreamGlassTranslationKeys.PROFILE_EDITOR_CATEGORY, "Category:" },
                { StreamGlassTranslationKeys.PROFILE_EDITOR_DESCRIPTION, "Description:" },
                { StreamGlassTranslationKeys.PROFILE_EDITOR_LANGUAGE, "Language:" },
                { StreamGlassTranslationKeys.PROFILE_EDITOR_CONTENT, "Content:" },
                { StreamGlassTranslationKeys.PROFILE_EDITOR_TIME, "Time:" },
                { StreamGlassTranslationKeys.PROFILE_EDITOR_MESSAGES, "Messages Number:" },
                { StreamGlassTranslationKeys.PROFILE_EDITOR_USER, "User:" },
                { StreamGlassTranslationKeys.PROFILE_EDITOR_ENABLE, "Enable:" },
                { StreamGlassTranslationKeys.PROFILE_EDITOR_TIME_DELTA, "Time Delta:" },
                { StreamGlassTranslationKeys.SECTION_STRING_SOURCES, "String Sources" },
                { StreamGlassTranslationKeys.STRING_SOURCE_EDITOR_PATH, "Path:" },
                { StreamGlassTranslationKeys.STRING_SOURCE_EDITOR_CONTENT, "Content:" },
                { StreamGlassTranslationKeys.SOUND_EDITOR_AUDIO_FILE, "File:" },
                { StreamGlassTranslationKeys.SOUND_EDITOR_AUDIO_OUTPUT, "Output:" },
            };
            Translator.AddTranslation(translation);
            Translation translationFR = new(new CultureInfo("fr-FR"), true)
            {
                { StreamGlassTranslationKeys.MENU_FILE, "Fichier" },
                { StreamGlassTranslationKeys.MENU_SETTINGS, "Paramètres" },
                { StreamGlassTranslationKeys.MENU_PROFILE, "Profiles" },
                { StreamGlassTranslationKeys.MENU_EDIT_STRING_SOURCES, "Éditer les sources de texte..." },
                { StreamGlassTranslationKeys.MENU_EDIT_PROFILE, "Éditer les profiles..." },
                { StreamGlassTranslationKeys.MENU_HELP, "Aide" },
                { StreamGlassTranslationKeys.MENU_ABOUT, "À Propos" },
                { StreamGlassTranslationKeys.APP_NAME, "Stream Glass" },
                { StreamGlassTranslationKeys.SETTINGS_GENERAL_COLOR, "Thème :" },
                { StreamGlassTranslationKeys.SETTINGS_GENERAL_LANGUAGE, "Language :" },
                { StreamGlassTranslationKeys.SAVE_BUTTON, "Sauvegarder" },
                { StreamGlassTranslationKeys.CLOSE_BUTTON, "Quitter" },
                { StreamGlassTranslationKeys.TEST_BUTTON, "Test" },
                { StreamGlassTranslationKeys.SECTION_PROFILES, "Profiles" },
                { StreamGlassTranslationKeys.SECTION_STREAM_INFO, "Info de Stream" },
                { StreamGlassTranslationKeys.SECTION_COMMANDS, "Commandes" },
                { StreamGlassTranslationKeys.SECTION_ALIASES, "Alias" },
                { StreamGlassTranslationKeys.SECTION_SUB_COMMANDS, "Sous-Commandes" },
                { StreamGlassTranslationKeys.SECTION_AUTO_TRIGGER, "Activation Auto" },
                { StreamGlassTranslationKeys.SECTION_DEFAULT_ARGUMENTS, "Arguments par Défaut" },
                { StreamGlassTranslationKeys.PROFILE_EDITOR_NAME, "Nom :" },
                { StreamGlassTranslationKeys.PROFILE_EDITOR_PARENT, "Parent :" },
                { StreamGlassTranslationKeys.PROFILE_EDITOR_IS_SELECTABLE, "Séléctionable :" },
                { StreamGlassTranslationKeys.PROFILE_EDITOR_TITLE, "Titre :" },
                { StreamGlassTranslationKeys.PROFILE_EDITOR_CATEGORY, "Catégorie :" },
                { StreamGlassTranslationKeys.PROFILE_EDITOR_DESCRIPTION, "Description :" },
                { StreamGlassTranslationKeys.PROFILE_EDITOR_LANGUAGE, "Language :" },
                { StreamGlassTranslationKeys.PROFILE_EDITOR_CONTENT, "Contenu :" },
                { StreamGlassTranslationKeys.PROFILE_EDITOR_TIME, "Temp :" },
                { StreamGlassTranslationKeys.PROFILE_EDITOR_MESSAGES, "Nombre de Message :" },
                { StreamGlassTranslationKeys.PROFILE_EDITOR_USER, "Utilisateur :" },
                { StreamGlassTranslationKeys.PROFILE_EDITOR_ENABLE, "Activer :" },
                { StreamGlassTranslationKeys.PROFILE_EDITOR_TIME_DELTA, "Delta de Temps :" },
                { StreamGlassTranslationKeys.SECTION_STRING_SOURCES, "Source de texte" },
                { StreamGlassTranslationKeys.STRING_SOURCE_EDITOR_PATH, "Chemin :" },
                { StreamGlassTranslationKeys.STRING_SOURCE_EDITOR_CONTENT, "Contenu :" },
                { StreamGlassTranslationKeys.SOUND_EDITOR_AUDIO_FILE, "Fichier :" },
                { StreamGlassTranslationKeys.SOUND_EDITOR_AUDIO_OUTPUT, "Sortie :" },
            };
            Translator.AddTranslation(translationFR);
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
