using StreamGlass.StreamChat;
using StreamFeedstock.Controls;
using System;
using System.Windows;

namespace StreamGlass.Profile
{
    public partial class ProfilesDialog : Dialog
    {
        private readonly ProfileManager m_ProfileManager;
        private readonly IStreamChat m_StreamChat;

        public ProfilesDialog(StreamFeedstock.Controls.Window parent, ProfileManager manager, IStreamChat streamChat): base(parent)
        {
            m_ProfileManager = manager;
            m_StreamChat = streamChat;
            InitializeComponent();
            ProfileList.ItemAdded += ProfileList_AddProfile;
            ProfileList.ItemRemoved += ProfileList_RemoveProfile;
            ProfileList.ItemEdited += ProfileList_EditProfile;
            ProfileList.SetConversionDelegate(ConvertProfile);

            foreach (Profile profile in manager.Objects)
            {
                if (profile.IsSerializable())
                    ProfileList.AddObject(profile);
            }
        }

        private string ConvertProfile(object profile) => ((Profile)profile).Name;

        private void ProfileList_AddProfile(object? sender, EventArgs _)
        {
            ProfileEditor dialog = new(this, m_ProfileManager, m_StreamChat);
            dialog.ShowDialog();
            Profile? newProfile = dialog.Profile;
            if (newProfile != null)
            {
                m_ProfileManager.AddProfile(newProfile);
                ProfileList.AddObject(newProfile);
            }
        }

        private void ProfileList_RemoveProfile(object? sender, object args) => m_ProfileManager.RemoveObject(((Profile)args).ID);
        
        private void ProfileList_EditProfile(object? sender, object args)
        {
            ProfileEditor dialog = new(this, m_ProfileManager, m_StreamChat, (Profile)args)
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            dialog.ShowDialog();
            Profile? editedProfile = dialog.Profile;
            if (editedProfile != null)
            {
                ProfileList.UpdateObject(args, editedProfile);
                m_ProfileManager.UpdateProfile(editedProfile);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
