using StreamGlass.Core.Controls;
using System.Windows;
using StreamGlass.Core.Connections;

namespace StreamGlass.Core.Profile
{
    public partial class ProfilesDialog : Dialog
    {
        private readonly ProfileManager m_ProfileManager;
        private readonly ConnectionManager m_ConnectionManager;

        public ProfilesDialog(Controls.Window parent, ProfileManager manager, ConnectionManager connectionManager): base(parent)
        {
            m_ProfileManager = manager;
            m_ConnectionManager = connectionManager;
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

        private void UpdateParent(Profile profile, string parentID)
        {
            if (!string.IsNullOrWhiteSpace(parentID))
            {
                Profile? parent = m_ProfileManager.GetObject(parentID);
                if (parent != null)
                    profile.SetParent(parent);
            }
        }

        private void ProfileList_AddProfile(object? sender, EventArgs _)
        {
            StatisticFileEditor dialog = new(this, m_ProfileManager, m_ConnectionManager);
            dialog.ShowDialog();
            Profile? newProfile = dialog.Profile;
            if (newProfile != null)
            {
                UpdateParent(newProfile, dialog.ParentID);
                m_ProfileManager.AddProfile(newProfile);
                ProfileList.AddObject(newProfile);
            }
        }

        private void ProfileList_RemoveProfile(object? sender, object args) => m_ProfileManager.RemoveObject(((Profile)args).ID);
        
        private void ProfileList_EditProfile(object? sender, object args)
        {
            StatisticFileEditor dialog = new(this, m_ProfileManager, m_ConnectionManager, (Profile)args);
            dialog.ShowDialog();
            Profile? editedProfile = dialog.Profile;
            if (editedProfile != null)
            {
                UpdateParent(editedProfile, dialog.ParentID);
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
