using StreamGlass.Core.Controls;
using System.Windows;

namespace StreamGlass.Core.Stat
{
    public partial class StringSourceFileDialog : Dialog
    {
        private readonly StringSourceManager m_StringSourceManager;

        public StringSourceFileDialog(Controls.Window parent): base(parent)
        {
            m_StringSourceManager = StreamGlassContext.StringSources;
            InitializeComponent();

            StringSourceFileGroupBox.SetTranslationKey(StreamGlassTranslationKeys.SECTION_STRING_SOURCES);

            StringSourceFileList.ItemAdded += StringSourceFileList_AddStringSourceFile;
            StringSourceFileList.ItemRemoved += StringSourceFileList_RemoveStringSourceFile;
            StringSourceFileList.ItemEdited += StringSourceFileList_EditStringSourceFile;
            StringSourceFileList.SetConversionDelegate(ConvertProfile);

            foreach (StringSourceFile stringSourceFile in m_StringSourceManager.Objects)
                StringSourceFileList.AddObject(stringSourceFile);
        }

        private string ConvertProfile(object profile) => ((StringSourceFile)profile).Name;

        private void StringSourceFileList_AddStringSourceFile(object? sender, EventArgs _)
        {
            StringSourceFileEditor dialog = new(this);
            dialog.ShowDialog();
            StringSourceFile? newStringSource = dialog.StringSourceFile;
            if (newStringSource != null)
            {
                m_StringSourceManager.AddStringSourceFile(newStringSource);
                StringSourceFileList.AddObject(newStringSource);
            }
        }

        private void StringSourceFileList_RemoveStringSourceFile(object? sender, object args) => m_StringSourceManager.RemoveObject(((StringSourceFile)args).ID);
        
        private void StringSourceFileList_EditStringSourceFile(object? sender, object args)
        {
            StringSourceFileEditor dialog = new(this, (StringSourceFile)args);
            dialog.ShowDialog();
            StringSourceFile? editedStringSource = dialog.StringSourceFile;
            if (editedStringSource != null)
            {
                StringSourceFileList.UpdateObject(args, editedStringSource);
                m_StringSourceManager.UpdateStringSourceFile(editedStringSource);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
