using System.Windows;
using StreamGlass.Core.Controls;

namespace StreamGlass.Core.Profile
{
    public partial class ChatCommandEditor : Dialog
    {
        private ChatCommand? m_CreatedCommand = null;

        public ChatCommandEditor(Controls.Window parent): base(parent)
        {
            InitializeComponent();
            UserUpDown.Value = 0;
            AliasesList.ItemAdded += EditableList_AddString;
            AliasesList.ItemEdited += EditableList_EditString;
            SubCommandList.ItemAdded += EditableList_AddString;
            SubCommandList.ItemEdited += EditableList_EditString;
            AutoTriggerArguments.ItemAdded += EditableList_AddString;
            AutoTriggerArguments.ItemEdited += EditableList_EditString;
        }

        public ChatCommandEditor(Controls.Window parent, ChatCommand command): this(parent)
        {
            NameTextBox.Text = command.Name;
            AliasesList.AddObjects(command.Aliases);
            TimeUpDown.Value = command.AwaitTime;
            NbMessageUpDown.Value = command.NbMessage;
            ContentTextBox.Text = command.Content;
            UserUpDown.Value = command.UserType;
            SubCommandList.AddObjects(command.Commands);

            AutoTriggerEnableCheckBox.IsChecked = command.AutoTrigger;
            AutoTriggerTimeUpDown.Value = command.AutoTriggerTime;
            AutoTriggerTimeDeltaUpDown.Value = command.AutoTriggerDeltaTime;
            AutoTriggerArguments.AddObjects(command.AutoTriggerArguments);

            UpdateAutoTriggerVisibility();
        }

        internal ChatCommand? Command => m_CreatedCommand;

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text;
            string[] aliases = AliasesList.GetItems().Cast<string>().ToArray();
            int awaitTime = (int)TimeUpDown.Value;
            int nbMessage = (int)NbMessageUpDown.Value;
            string content = ContentTextBox.Text;
            uint userType = (uint)UserUpDown.Value;
            List<string> commands = SubCommandList.GetItems().Cast<string>().ToList();
            bool autoTrigger = AutoTriggerEnableCheckBox.IsChecked ?? false;
            int autoTriggerTime = (int)AutoTriggerTimeUpDown.Value;
            int autoTriggerDeltaTime = (int)AutoTriggerTimeDeltaUpDown.Value;
            string[] autoTriggerArguments = AutoTriggerArguments.GetItems().Cast<string>().ToArray();
            m_CreatedCommand = new(name, aliases, awaitTime, nbMessage, content, userType, commands, autoTrigger, autoTriggerTime, autoTriggerDeltaTime, autoTriggerArguments);
            OnOkClick();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            m_CreatedCommand = null;
            OnCancelClick();
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
