using StreamFeedstock.Controls;
using System;
using System.Collections.Generic;
using System.Windows;
using StreamGlass.Connections;

namespace StreamGlass.Profile
{
    public partial class ProfileEditor : Dialog
    {
        private Profile? m_CreatedProfile = null;
        private readonly Dictionary<string, ChatCommand> m_ChatCommands = new();
        private readonly ConnectionManager m_ConnectionManager;
        private readonly CategoryInfo m_CategoryInfo = new("");
        private readonly string m_ID = Guid.NewGuid().ToString();

        public ProfileEditor(StreamFeedstock.Controls.Window parent, ProfileManager profileManager, ConnectionManager connectionManager): base(parent)
        {
            m_ConnectionManager = connectionManager;
            InitializeComponent();
            ChatCommandsList.SetConversionDelegate(ConvertCommand);
            ChatCommandsList.ItemAdded += ChatCommandsList_AddChatCommand;
            ChatCommandsList.ItemRemoved += ChatCommandsList_RemoveChatCommand;
            ChatCommandsList.ItemEdited += ChatCommandsList_EditChatCommand;
            profileManager.FillComboBox(ref ParentComboBox);
            ParentComboBox.SelectedIndex = 0;
            IsSelectableCheckBox.IsChecked = true;
        }

        public ProfileEditor(StreamFeedstock.Controls.Window parent, ProfileManager profileManager, ConnectionManager connectionManager, Profile profile): this(parent, profileManager, connectionManager)
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
        internal string ParentID => ((Profile.Info?)ParentComboBox.SelectedItem)?.ID ?? "";

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
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            m_CreatedProfile = null;
            Close();
        }

        private void StreamInfoCategorySearchButton_Click(object sender, RoutedEventArgs e)
        {
            CategoryInfo? searchedCategory = m_ConnectionManager.SearchCategoryInfo(this, m_CategoryInfo);
            if (searchedCategory != null)
            {
                m_CategoryInfo.Copy(searchedCategory);
                StreamInfoCategoryTextBox.Text = m_CategoryInfo.Name;
            }
        }
    }
}
