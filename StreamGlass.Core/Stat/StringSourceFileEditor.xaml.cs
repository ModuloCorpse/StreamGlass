using StreamGlass.Core.Controls;
using System.IO;
using System.Windows;

namespace StreamGlass.Core.Stat
{
    public partial class StringSourceFileEditor : Dialog
    {
        private StringSourceFile? m_CreatedStringSourceFile = null;
        private readonly string m_ID = Guid.NewGuid().ToString();


        //TODO Use placeholder analytics tool to extract all variables from content
        public StringSourceFileEditor(Controls.Window parent): base(parent)
        {
            InitializeComponent();

            PathTextBoxLabel.SetTranslationKey(StreamGlassTranslationKeys.STRING_SOURCE_EDITOR_PATH);
            ContentTextBoxLabel.SetTranslationKey(StreamGlassTranslationKeys.STRING_SOURCE_EDITOR_CONTENT);
            StringSourceGroupBox.SetTranslationKey(StreamGlassTranslationKeys.SECTION_STRING_SOURCES);
            SaveButton.SetTranslationKey(StreamGlassTranslationKeys.SAVE_BUTTON);

            StringSourcesList.ItemAdded += EditableList_AddString;
            StringSourcesList.ItemEdited += EditableList_EditString;
        }

        public StringSourceFileEditor(Controls.Window parent, StringSourceFile statisticFile): this(parent)
        {
            m_ID = statisticFile.ID;
            PathTextBox.Text = statisticFile.Path;
            ContentTextBox.Text = statisticFile.Content;
            StringSourcesList.AddObjects(statisticFile.Sources);
        }

        internal StringSourceFile? StringSourceFile => m_CreatedStringSourceFile;

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            StringSourceFile newStatisticFile = new(m_ID, PathTextBox.Text);
            newStatisticFile.SetContent(ContentTextBox.Text);
            string[] statistics = StringSourcesList.GetItems().Cast<string>().ToArray();
            foreach (string statistic in statistics)
                newStatisticFile.AddSource(statistic);
            m_CreatedStringSourceFile = newStatisticFile;
            OnOkClick();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            m_CreatedStringSourceFile = null;
            OnCancelClick();
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
