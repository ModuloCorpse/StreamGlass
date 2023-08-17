using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using CorpseLib.Translation;
using StreamGlass;
using StreamGlass.Controls;
using static StreamGlass.StreamChat.UserMessage;

namespace StreamGlass.Profile
{
    public partial class ChatCommandEditor : Dialog
    {
        private ChatCommand? m_CreatedCommand = null;

        public ChatCommandEditor(StreamGlass.Controls.Window parent): base(parent)
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
            AliasesList.ItemAdded += EditableList_AddString;
            AliasesList.ItemEdited += EditableList_EditString;
            SubCommandList.ItemAdded += EditableList_AddString;
            SubCommandList.ItemEdited += EditableList_EditString;
            AutoTriggerArguments.ItemAdded += EditableList_AddString;
            AutoTriggerArguments.ItemEdited += EditableList_EditString;
        }

        public ChatCommandEditor(StreamGlass.Controls.Window parent, ChatCommand command): this(parent)
        {
            NameTextBox.Text = command.Name;
            AliasesList.AddObjects(command.Aliases);
            TimeUpDown.Value = command.AwaitTime;
            NbMessageUpDown.Value = command.NbMessage;
            ContentTextBox.Text = command.Content;
            UserComboBox.SelectedIndex = (int)command.UserType;
            SubCommandList.AddObjects(command.Commands);

            AutoTriggerEnableCheckBox.IsChecked = command.AutoTrigger;
            AutoTriggerTimeUpDown.Value = command.AutoTriggerTime;
            AutoTriggerTimeDeltaUpDown.Value = command.AutoTriggerDeltaTime;
            AutoTriggerArguments.AddObjects(command.AutoTriggerArguments);

            UpdateAutoTriggerVisibility();
        }

        private void TranslateComboBox()
        {
            int selectedIndex = UserComboBox.SelectedIndex;
            UserComboBox.Items.Clear();
            UserComboBox.Items.Add(Translator.Translate("${user_type_none}"));
            UserComboBox.Items.Add(Translator.Translate("${user_type_mod}"));
            UserComboBox.Items.Add(Translator.Translate("${user_type_global_mod}"));
            UserComboBox.Items.Add(Translator.Translate("${user_type_admin}"));
            UserComboBox.Items.Add(Translator.Translate("${user_type_staff}"));
            UserComboBox.Items.Add(Translator.Translate("${user_type_broadcaster}"));
            UserComboBox.Items.Add(Translator.Translate("${user_type_self}"));
            UserComboBox.SelectedIndex = selectedIndex;
        }

        protected override void OnUpdate(BrushPaletteManager palette)
        {
            base.OnUpdate(palette);
            TranslateComboBox();
        }

        internal ChatCommand? Command => m_CreatedCommand;

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text;
            string[] aliases = AliasesList.GetItems().Cast<string>().ToArray();
            int awaitTime = (int)TimeUpDown.Value;
            int nbMessage = (int)NbMessageUpDown.Value;
            string content = ContentTextBox.Text;
            User.Type userType = (User.Type)UserComboBox.SelectedIndex;
            List<string> commands = SubCommandList.GetItems().Cast<string>().ToList();
            bool autoTrigger = AutoTriggerEnableCheckBox.IsChecked ?? false;
            int autoTriggerTime = (int)AutoTriggerTimeUpDown.Value;
            int autoTriggerDeltaTime = (int)AutoTriggerTimeDeltaUpDown.Value;
            string[] autoTriggerArguments = AutoTriggerArguments.GetItems().Cast<string>().ToArray();
            m_CreatedCommand = new(name, aliases, awaitTime, nbMessage, content, userType, commands, autoTrigger, autoTriggerTime, autoTriggerDeltaTime, autoTriggerArguments);
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
