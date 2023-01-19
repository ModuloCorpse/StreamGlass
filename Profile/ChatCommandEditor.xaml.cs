using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using StreamFeedstock;
using StreamFeedstock.Controls;
using static StreamGlass.StreamChat.UserMessage;

namespace StreamGlass.Profile
{
    public partial class ChatCommandEditor : Dialog
    {
        private ChatCommand? m_CreatedCommand = null;

        public ChatCommandEditor(StreamFeedstock.Controls.Window parent): base(parent)
        {
            InitializeComponent();
            UserComboBox.Items.Add("Viewer");
            UserComboBox.Items.Add("Moderator");
            UserComboBox.Items.Add("Platform Moderator");
            UserComboBox.Items.Add("Platform Administrator");
            UserComboBox.Items.Add("Platform Staff");
            UserComboBox.Items.Add("Streamer");
            UserComboBox.Items.Add("Bot");
            UserComboBox.SelectedIndex = 0;
            SubCommandList.ItemAdded += EditableList_AddString;
            SubCommandList.ItemEdited += EditableList_EditString;
            AutoTriggerArguments.ItemAdded += EditableList_AddString;
            AutoTriggerArguments.ItemEdited += EditableList_EditString;
        }

        public ChatCommandEditor(StreamFeedstock.Controls.Window parent, ChatCommand command): this(parent)
        {
            NameTextBox.Text = command.Name;
            TimeUpDown.Value = command.AwaitTime;
            NbMessageUpDown.Value = command.NbMessage;
            NbArgumentsUpDown.Value = command.NBArguments;
            ContentTextBox.Text = command.Content;
            UserComboBox.SelectedIndex = (int)command.UserType;
            SubCommandList.AddObjects(command.Commands);

            AutoTriggerEnableCheckBox.IsChecked = command.AutoTrigger;
            AutoTriggerTimeUpDown.Value = command.AutoTriggerTime;
            AutoTriggerTimeDeltaUpDown.Value = command.AutoTriggerDeltaTime;
            AutoTriggerArguments.AddObjects(command.AutoTriggerArguments);

            UpdateAutoTriggerVisibility();
        }

        private void AddUserType(TranslationManager translation, string key, string defaultVal)
        {
            if (translation.TryGetTranslation(key, out var ret))
                UserComboBox.Items.Add(ret);
            else
                UserComboBox.Items.Add(defaultVal);
        }

        private void TranslateComboBox(TranslationManager translation)
        {
            int selectedIndex = UserComboBox.SelectedIndex;
            UserComboBox.Items.Clear();
            AddUserType(translation, "user_type_none", "Viewer");
            AddUserType(translation, "user_type_mod", "Moderator");
            AddUserType(translation, "user_type_global_mod", "Platform Moderator");
            AddUserType(translation, "user_type_admin", "Platform Administrator");
            AddUserType(translation, "user_type_staff", "Platform Staff");
            AddUserType(translation, "user_type_broadcaster", "Streamer");
            AddUserType(translation, "user_type_self", "Bot");
            UserComboBox.SelectedIndex = selectedIndex;
        }

        protected override void OnUpdate(BrushPaletteManager palette, TranslationManager translation)
        {
            base.OnUpdate(palette, translation);
            TranslateComboBox(translation);
        }

        internal ChatCommand? Command => m_CreatedCommand;

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text;
            int awaitTime = (int)TimeUpDown.Value;
            int nbMessage = (int)NbMessageUpDown.Value;
            int nbArguments = (int)NbArgumentsUpDown.Value;
            string content = ContentTextBox.Text;
            UserType userType = (UserType)UserComboBox.SelectedIndex;
            List<string> commands = SubCommandList.GetItems().Cast<string>().ToList();
            bool autoTrigger = AutoTriggerEnableCheckBox.IsChecked ?? false;
            int autoTriggerTime = (int)AutoTriggerTimeUpDown.Value;
            int autoTriggerDeltaTime = (int)AutoTriggerTimeDeltaUpDown.Value;
            string[] autoTriggerArguments = AutoTriggerArguments.GetItems().Cast<string>().ToArray();
            m_CreatedCommand = new(name, awaitTime, nbMessage, nbArguments, content, userType, commands, autoTrigger, autoTriggerTime, autoTriggerDeltaTime, autoTriggerArguments);
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            m_CreatedCommand = null;
            Close();
        }

        private void UpdateAutoTriggerVisibility()
        {
            if (AutoTriggerEnableCheckBox.IsChecked ?? false)
            {
                AutoTriggerTimePanel.Visibility = Visibility.Visible;
                AutoTriggerDeltaTimePanel.Visibility = Visibility.Visible;
                AutoTriggerDefaultArgumentGroupBox.Visibility = Visibility.Visible;
            }
            else
            {
                AutoTriggerTimePanel.Visibility = Visibility.Collapsed;
                AutoTriggerDeltaTimePanel.Visibility = Visibility.Collapsed;
                AutoTriggerDefaultArgumentGroupBox.Visibility = Visibility.Collapsed;
            }
        }

        private void AutoTriggerEnableCheckBox_Checked(object sender, RoutedEventArgs e) => UpdateAutoTriggerVisibility();

        private void EditableList_AddString(object? sender, EventArgs _)
        {
            StringEditor dialog = new(this);
            dialog.ShowDialog();
            string? newStr = dialog.Str;
            if (newStr != null)
            {
                EditableList list = (EditableList)sender!;
                list.AddObject(newStr);
            }
        }

        private void EditableList_EditString(object? sender, object args)
        {
            string oldStr = (string)args;
            StringEditor dialog = new(this, oldStr);
            dialog.ShowDialog();
            string? newStr = dialog.Str;
            if (newStr != null)
            {
                EditableList list = (EditableList)sender!;
                list.UpdateObject(args, newStr);
            }
        }
    }
}
