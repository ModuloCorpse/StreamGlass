using CorpseLib.DataNotation;
using CorpseLib.Json;
using CorpseLib.Translation;
using CorpseLib.Web.API;
using CorpseLib.Web.API.Event;
using StreamGlass.API.Overlay.Chat;
using StreamGlass.Core;
using StreamGlass.Core.API;
using StreamGlass.Core.API.Event;
using StreamGlass.Core.API.Overlay;
using StreamGlass.Core.Profile;
using StreamGlass.Twitch;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace StreamGlass
{
    public class Manager
    {
        static Manager()
        {
            DataHelper.RegisterSerializer(new Settings.ChatSettings.DataSerializer());
            DataHelper.RegisterSerializer(new Settings.DataSerializer());
        }

        private readonly CorpseLib.Web.API.API m_API = new(15007);
        private readonly TickManager m_TickManager = new();
        private Settings m_Settings = new();
        private readonly ProfileManager m_ProfileManager;
        private readonly PluginManager m_PluginManager;

        public ProfileManager ProfileManager => m_ProfileManager;

        public Manager(SplashScreen splashScreen)
        {
            StreamGlassChat.Init(m_TickManager);

            //Progress bar message: Learning new languages
            LoadSettings();

            StreamGlassChat.StreamChatPanel.SetDisplayType(m_Settings.Chat.DisplayType);
            StreamGlassChat.StreamChatPanel.SetContentFontSize(m_Settings.Chat.MessageFontSize);

            InitializeTranslation();
            Translator.LoadDirectory("./locals");
            Translator.SetLanguage(new CultureInfo(m_Settings.CurrentLanguage));
            splashScreen.UpdateProgressBar(50);

            //Progress bar translated message: Loading profiles
            m_ProfileManager = new(m_TickManager);
            m_ProfileManager.Load();
            StreamGlassChat.RegisterMessageReceiver(m_ProfileManager);
            splashScreen.UpdateProgressBar(60);

            PluginManager.PLUGIN_LOGGER.Start();
            m_PluginManager = new(m_TickManager);
            //Progress bar translated message: Loading XX (name of the plugin)
            m_PluginManager.LoadPlugins();
            //Progress bar translated message: Loading Twitch
            m_PluginManager.LoadPlugin(TwitchPlugin.PluginMetadata, new TwitchPlugin());
            splashScreen.UpdateProgressBar(70);

            m_ProfileManager.UpdateStreamInfo();

            //Progress bar translated message: Loading API endpoints
            InitAPI();
            m_API.Start();

            foreach (var pair in m_API.FlattenEndpoints())
            {
                AEndpoint endpoint = pair.Value;
                StringBuilder builder = new("- ");
                if (endpoint.IsHTTPEndpoint)
                    builder.Append("HTTP ");
                if (endpoint.IsWebsocketEndpoint)
                    builder.Append("Websocket ");
                builder.Append(pair.Key);
                builder.Append(" => ");
                builder.Append(endpoint.GetType().Name);
                StreamGlassContext.LOGGER.Log(builder.ToString());
            }

            splashScreen.UpdateProgressBar(80);

            //Last initalization to do
            //Progress bar translated message: Initializing XX (name of the plugin)
            m_PluginManager.InitPlugins();

            //Progress bar translated message: Starting update routine
            m_TickManager.Start();
            splashScreen.UpdateProgressBar(90);
        }

        private void LoadSettings()
        {
            if (File.Exists("settings.json"))
            {
                Settings? settings = JsonParser.LoadFromFile<Settings>("settings.json");
                if (settings != null)
                    m_Settings = settings;
            }
        }

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
                { StreamGlassTranslationKeys.PROFILE_EDITOR_SHARE, "Multi-chat:" },
                { StreamGlassTranslationKeys.SECTION_STRING_SOURCES, "String Sources" },
                { StreamGlassTranslationKeys.STRING_SOURCE_EDITOR_PATH, "Path:" },
                { StreamGlassTranslationKeys.STRING_SOURCE_EDITOR_CONTENT, "Content:" },
                { StreamGlassTranslationKeys.SOUND_EDITOR_AUDIO_FILE, "File:" },
                { StreamGlassTranslationKeys.SOUND_EDITOR_AUDIO_OUTPUT, "Output:" },
                { StreamGlassTranslationKeys.SOUND_EDITOR_AUDIO_COOLDOWN, "Cooldown (s):" },
                { StreamGlassTranslationKeys.SETTINGS_CHAT_MODE, "Chat Mode:" },
                { StreamGlassTranslationKeys.SETTINGS_CHAT_FONT, "Chat Font Size:" },
                { StreamGlassTranslationKeys.CHAT_TOGGLE_HIGHLIGHT_USER, "Toggle user highlight" },
                { StreamGlassTranslationKeys.CHAT_DELETE_MESSAGE, "Delete message" },
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
                { StreamGlassTranslationKeys.PROFILE_EDITOR_SHARE, "Multi-chat :" },
                { StreamGlassTranslationKeys.SECTION_STRING_SOURCES, "Source de texte" },
                { StreamGlassTranslationKeys.STRING_SOURCE_EDITOR_PATH, "Chemin :" },
                { StreamGlassTranslationKeys.STRING_SOURCE_EDITOR_CONTENT, "Contenu :" },
                { StreamGlassTranslationKeys.SOUND_EDITOR_AUDIO_FILE, "Fichier :" },
                { StreamGlassTranslationKeys.SOUND_EDITOR_AUDIO_OUTPUT, "Sortie :" },
                { StreamGlassTranslationKeys.SOUND_EDITOR_AUDIO_COOLDOWN, "Attente (s) :" },
                { StreamGlassTranslationKeys.SETTINGS_CHAT_MODE, "Mode du Chat :" },
                { StreamGlassTranslationKeys.SETTINGS_CHAT_FONT, "Taille du Chat :" },
                { StreamGlassTranslationKeys.CHAT_TOGGLE_HIGHLIGHT_USER, "Mises en avant" },
                { StreamGlassTranslationKeys.CHAT_DELETE_MESSAGE, "Supprimer le message" },
            };
            Translator.AddTranslation(translationFR);
            Translator.CurrentLanguageChanged += () => m_Settings.CurrentLanguage = Translator.CurrentLanguage.Name;
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

            OverlayEndpoint overlayEndpoint = new();
            // '/overlay'
            Overlay defaultOverlay = new();
            defaultOverlay.AddAssemblyResource("StreamGlass.js", "StreamGlass.API.Overlay.StreamGlass.js");
            overlayEndpoint.AddOverlay(new(), defaultOverlay);
            // '/overlay/chat'
            Overlay chatOverlay = new("chat");
            chatOverlay.AddRootResource("chat.html", new ChatRootEndpoint());
            chatOverlay.AddAssemblyResource("chat.css", "StreamGlass.API.Overlay.Chat.chat.css");
            chatOverlay.AddAssemblyResource("chat.js", "StreamGlass.API.Overlay.Chat.chat.js");
            overlayEndpoint.AddOverlay(new("chat"), chatOverlay);
            m_PluginManager.RegisterOverlays(overlayEndpoint);

            //Local overlay will always be loaded last to allow overriding plugin overlay
            if (Directory.Exists("overlay/"))
            {
                CorpseLib.Web.Http.Path localPath = new("local");
                string[] directories = Directory.GetDirectories("overlay/");
                foreach (string directory in directories)
                {
                    LocalOverlay localOverlay = new(directory);
                    overlayEndpoint.AddOverlay(CorpseLib.Web.Http.Path.Append(localPath, localOverlay.RootPath), localOverlay);
                }
            }
            m_API.AddEndpoint(overlayEndpoint);

            m_PluginManager.RegisterToAPI(m_API);
        }

        public void Stop()
        {
            DataObject json = (DataObject)DataHelper.Cast(m_Settings);
            StreamGlassContext.Delete(json);
            JsonParser.WriteToFile("settings.json", json);
            m_ProfileManager.Save();
            m_PluginManager.Clear();
            m_TickManager.Stop();
            m_API.Stop();
        }

        public void FillSettingsDialog(Core.Settings.Dialog settingsDialog)
        {
            settingsDialog.AddTabItem(new StreamChatSettingsItem(m_Settings.Chat, StreamGlassChat.StreamChatPanel));
            m_PluginManager.FillSettings(settingsDialog);
        }

        public void Test() => m_PluginManager.Test();

        public Control? GetPanel(string panelID)
        {
            return panelID switch
            {
                //TODO
                "chat" => StreamGlassChat.StreamChatPanel,
                _ => m_PluginManager.GetPanel(panelID)
            };
        }
    }
}
