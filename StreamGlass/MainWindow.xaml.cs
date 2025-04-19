using CorpseLib.Translation;
using StreamGlass.Core;
using StreamGlass.Core.Controls;
using StreamGlass.Core.Profile;
using StreamGlass.Core.Stat;
using StreamGlass.StreamChat;
using System;
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
        }

        public MainWindow(SplashScreen splashScreen) : base(new())
        {
            //Progress bar message: Waking up
            m_SplashScreen = splashScreen;
            StreamGlassContext.Init();
            StreamGlassContext.LOGGER.Start();
            StreamGlassCanals.Register<string>(StreamGlassCanals.PROFILE_CHANGED_MENU_ITEM, (profileID) => m_MenuItemsRadioGroup.Select(profileID!));

            InitializeComponent();

            FileMenuItem.SetTranslationKey(StreamGlassTranslationKeys.MENU_FILE);
            SettingsMenuItem.SetTranslationKey(StreamGlassTranslationKeys.MENU_SETTINGS);
            ProfilesMenuItem.SetTranslationKey(StreamGlassTranslationKeys.MENU_PROFILE);
            StringSourcesMenuEdit.SetTranslationKey(StreamGlassTranslationKeys.MENU_EDIT_STRING_SOURCES);
            ProfileMenuEdit.SetTranslationKey(StreamGlassTranslationKeys.MENU_EDIT_PROFILE);
            HelpMenuItem.SetTranslationKey(StreamGlassTranslationKeys.MENU_HELP);
            HelpAboutMenuItem.SetTranslationKey(StreamGlassTranslationKeys.MENU_ABOUT);
            AppNameLabel.SetTranslationKey(StreamGlassTranslationKeys.APP_NAME);

            m_SplashScreen.UpdateProgressBar(10);
            InitializeBrushPalette();
            m_SplashScreen.UpdateProgressBar(30);
            m_Manager = new(splashScreen);

            //Progress bar translated message: Interface loading
            StreamGlassContext.AfterPluginInit();
            //TODO Temporary code : To replace with AvalonDock layouts
            StreamChatBorder.Child = m_Manager.GetPanel("chat");
            StreamAlertBorder.Child = m_Manager.GetPanel("twitch_alert");
            HeldMessageBorder.Child = m_Manager.GetPanel("twitch_held_message");
            //
            UpdateProfilesMenuList();

            m_SplashScreen.UpdateProgressBar(100);
        }

        internal void UpdateProfilesMenuList()
        {
            ProfilesMenuItem.Items.Clear();
            ProfilesMenuItem.Items.Add(StringSourcesMenuEdit);
            ProfilesMenuItem.Items.Add(ProfileMenuEdit);
            ProfilesMenuItem.Items.Add(ProfilesMenuSeparator);

            m_MenuItemsRadioGroup.Clear();
            var profiles = m_Manager.ProfileManager.Objects;
            foreach (var profile in profiles)
            {
                if (profile.IsSelectable)
                {
                    ProfileMenuItem item = new(m_Manager.ProfileManager, profile.ID) { Header = profile.Name };
                    ProfilesMenuItem.Items.Add(item);
                    m_MenuItemsRadioGroup.AddItem(item);
                }
            }
            ProfilesMenuItem.Update(GetBrushPalette());
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            GetBrushPalette().Save();
            Translator.SaveToDir("./locals");
            m_Manager.Stop();
            Application.Current.Shutdown();
        }

        private void SettingsToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Core.Settings.Dialog dialog = new(this);
            dialog.AddTabItem(new GeneralSettingsItem(dialog, GetBrushPalette()));
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

        private void EditStringSourcesButton_Click(object sender, RoutedEventArgs e)
        {
            StringSourceFileDialog dialog = new(this);
            dialog.ShowDialog();
        }

        private void EditProfilesButton_Click(object sender, RoutedEventArgs e)
        {
            ProfilesDialog dialog = new(this, m_Manager.ProfileManager);
            dialog.ShowDialog();
            UpdateProfilesMenuList();
        }

        private void TestMenuItem_Click(object sender, RoutedEventArgs e)
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
