using StreamGlass.Core.Controls;
using System.Windows;

namespace StreamGlass.Core.Profile
{
    public partial class ProfileEditor : Dialog
    {
        public delegate CategoryInfo? ProfileSearchCategoryDelegate(Controls.Window parent, CategoryInfo? info);
        private static ProfileSearchCategoryDelegate ms_SearchCategoryDelegate = (_, __) => null;
        public static void SetSearchCategoryDelegate(ProfileSearchCategoryDelegate searchCategoryDelegate) => ms_SearchCategoryDelegate = searchCategoryDelegate;

        private Profile? m_CreatedProfile = null;
        private readonly Dictionary<string, ChatCommand> m_ChatCommands = [];
        private readonly CategoryInfo m_CategoryInfo = new(string.Empty);
        private readonly string m_ID = Guid.NewGuid().ToString();

        public ProfileEditor(Controls.Window parent, ProfileManager profileManager): base(parent)
        {
            InitializeComponent();

            NameTextBoxLabel.SetTranslationKey(StreamGlassTranslationKeys.PROFILE_EDITOR_NAME);
            ParentComboBoxLabel.SetTranslationKey(StreamGlassTranslationKeys.PROFILE_EDITOR_PARENT);
            IsSelectableCheckBoxLabel.SetTranslationKey(StreamGlassTranslationKeys.PROFILE_EDITOR_IS_SELECTABLE);
            StreamInfoGroupBox.SetTranslationKey(StreamGlassTranslationKeys.SECTION_STREAM_INFO);
            StreamInfoTitleTextBoxLabel.SetTranslationKey(StreamGlassTranslationKeys.PROFILE_EDITOR_TITLE);
            StreamInfoCategoryTextBoxLabel.SetTranslationKey(StreamGlassTranslationKeys.PROFILE_EDITOR_CATEGORY);
            StreamInfoDescriptionTextBoxLabel.SetTranslationKey(StreamGlassTranslationKeys.PROFILE_EDITOR_DESCRIPTION);
            StreamInfoLanguageTextBoxLabel.SetTranslationKey(StreamGlassTranslationKeys.PROFILE_EDITOR_LANGUAGE);
            CommandGroupBox.SetTranslationKey(StreamGlassTranslationKeys.SECTION_COMMANDS);
            SaveButton.SetTranslationKey(StreamGlassTranslationKeys.SAVE_BUTTON);

            ChatCommandsList.SetConversionDelegate(ConvertCommand);
            ChatCommandsList.ItemAdded += ChatCommandsList_AddChatCommand;
            ChatCommandsList.ItemRemoved += ChatCommandsList_RemoveChatCommand;
            ChatCommandsList.ItemEdited += ChatCommandsList_EditChatCommand;
            Helper.FillComboBox(profileManager, ref ParentComboBox);
            ParentComboBox.SelectedIndex = 0;
            IsSelectableCheckBox.IsChecked = true;
        }

        public ProfileEditor(Controls.Window parent, ProfileManager profileManager, Profile profile): this(parent, profileManager)
        {
            ParentComboBox.Items.Remove(profile.ObjectInfo);
            m_ID = profile.ID;
            NameTextBox.Text = profile.Name;
            Profile? profileParent = profile.Parent;
            if (profileParent != null)
                ParentComboBox.SelectedItem = profileParent.ObjectInfo;
            m_CategoryInfo.Copy(profile.GetStreamCategory());
            StreamInfoTitleTextBox.Text = profile.GetStreamTitle();
            StreamInfoCategoryTextBox.Text = m_CategoryInfo.Name;
            StreamInfoDescriptionTextBox.Text = profile.GetStreamDescription();
            StreamInfoLanguageTextBox.Text = profile.GetStreamLanguage();
            IsSelectableCheckBox.IsChecked = profile.IsSelectable;
            foreach (ChatCommand chatCommand in profile.Commands)
                AddCommand(chatCommand);
        }

        private string ConvertCommand(object chatCommand) => string.Format("!{0}", ((ChatCommand)chatCommand).Name);

        private void AddCommand(ChatCommand chatCommand)
        {
            ChatCommandsList.AddObject(chatCommand);
            m_ChatCommands[chatCommand.Name] = chatCommand;
        }

        internal Profile? Profile => m_CreatedProfile;
        internal string ParentID => ((Profile.Info?)ParentComboBox.SelectedItem)?.ID ?? string.Empty;

        private void ChatCommandsList_AddChatCommand(object? sender, EventArgs _)
        {
            ChatCommandEditor dialog = new(this);
            dialog.ShowDialog();
            ChatCommand? newCommand = dialog.Command;
            if (newCommand != null)
            {
                if (!m_ChatCommands.ContainsKey(newCommand.Name))
                    AddCommand(newCommand);
                else
                {
                    //TODO Error message
                }
            }
        }

        private void ChatCommandsList_RemoveChatCommand(object? sender, object args)
        {
            m_ChatCommands.Remove(((ChatCommand)args).Name);
        }

        private void ChatCommandsList_EditChatCommand(object? sender, object args)
        {
            ChatCommand chatCommand = (ChatCommand)args;
            ChatCommandEditor dialog = new(this, chatCommand);
            dialog.ShowDialog();
            ChatCommand? editedCommand = dialog.Command;
            if (editedCommand != null)
            {
                if (editedCommand.Name != chatCommand.Name)
                {
                    if (m_ChatCommands.ContainsKey(editedCommand.Name))
                    {
                        //TODO Error message
                        return;
                    }
                }
                ChatCommandsList.UpdateObject(args, editedCommand);
                m_ChatCommands[editedCommand.Name] = editedCommand;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Profile newProfile = new(m_ID, NameTextBox.Text);
            newProfile.SetIsSelectable(IsSelectableCheckBox.IsChecked ?? true);
            newProfile.SaveStreamInfo(StreamInfoTitleTextBox.Text, StreamInfoDescriptionTextBox.Text, m_CategoryInfo, StreamInfoLanguageTextBox.Text);
            foreach (ChatCommand command in m_ChatCommands.Values)
                newProfile.AddCommand(command);
            m_CreatedProfile = newProfile;
            OnOkClick();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            m_CreatedProfile = null;
            OnCancelClick();
        }

        private void StreamInfoCategorySearchButton_Click(object sender, RoutedEventArgs e)
        {
            CategoryInfo? searchedCategory = ms_SearchCategoryDelegate(this, m_CategoryInfo);
            if (searchedCategory != null)
            {
                m_CategoryInfo.Copy(searchedCategory);
                StreamInfoCategoryTextBox.Text = m_CategoryInfo.Name;
            }
        }
    }
}
