using StreamGlass.Controls;
using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace StreamGlass.Stat
{
    public partial class StatisticFileEditor : Dialog
    {
        private StatisticFile? m_CreatedStatisticFile = null;
        private readonly string m_ID = Guid.NewGuid().ToString();

        public StatisticFileEditor(Controls.Window parent): base(parent)
        {
            InitializeComponent();
            StatisticsList.ItemAdded += EditableList_AddString;
            StatisticsList.ItemEdited += EditableList_EditString;
        }

        public StatisticFileEditor(Controls.Window parent, StatisticFile statisticFile): this(parent)
        {
            m_ID = statisticFile.ID;
            PathTextBox.Text = statisticFile.Path;
            ContentTextBox.Text = statisticFile.Content;
            StatisticsList.AddObjects(statisticFile.Statistics);
        }

        internal StatisticFile? StatisticFile => m_CreatedStatisticFile;

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            StatisticFile newStatisticFile = new(m_ID, PathTextBox.Text);
            newStatisticFile.SetContent(ContentTextBox.Text);
            string[] statistics = StatisticsList.GetItems().Cast<string>().ToArray();
            foreach (string statistic in statistics)
                newStatisticFile.AddStatistic(statistic);
            m_CreatedStatisticFile = newStatisticFile;
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            m_CreatedStatisticFile = null;
            Close();
        }

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

        private void PathClipboardButton_Click(object sender, RoutedEventArgs e)
        {
            string? fileFullPath = Path.GetDirectoryName(Path.GetFullPath(PathTextBox.Text));
            Clipboard.SetText(fileFullPath);
        }
    }
}
