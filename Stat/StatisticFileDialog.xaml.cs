using StreamGlass.Controls;
using System;
using System.Windows;

namespace StreamGlass.Stat
{
    public partial class StatisticFileDialog : Dialog
    {
        private readonly StatisticManager m_StatisticManager;

        public StatisticFileDialog(Controls.Window parent): base(parent)
        {
            m_StatisticManager = StreamGlassContext.Statistics;
            InitializeComponent();
            StatisticFileList.ItemAdded += StatisticFileList_AddStatisticFile;
            StatisticFileList.ItemRemoved += StatisticFileList_RemoveStatisticFile;
            StatisticFileList.ItemEdited += StatisticFileList_EditStatisticFile;
            StatisticFileList.SetConversionDelegate(ConvertProfile);

            foreach (StatisticFile statisticFile in m_StatisticManager.Objects)
                StatisticFileList.AddObject(statisticFile);
        }

        private string ConvertProfile(object profile) => ((StatisticFile)profile).Name;

        private void StatisticFileList_AddStatisticFile(object? sender, EventArgs _)
        {
            StatisticFileEditor dialog = new(this);
            dialog.ShowDialog();
            StatisticFile? newProfile = dialog.StatisticFile;
            if (newProfile != null)
            {
                m_StatisticManager.AddStatisticFile(newProfile);
                StatisticFileList.AddObject(newProfile);
            }
        }

        private void StatisticFileList_RemoveStatisticFile(object? sender, object args) => m_StatisticManager.RemoveObject(((StatisticFile)args).ID);
        
        private void StatisticFileList_EditStatisticFile(object? sender, object args)
        {
            StatisticFileEditor dialog = new(this, (StatisticFile)args);
            dialog.ShowDialog();
            StatisticFile? editedProfile = dialog.StatisticFile;
            if (editedProfile != null)
            {
                StatisticFileList.UpdateObject(args, editedProfile);
                m_StatisticManager.UpdateStatisticFile(editedProfile);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
