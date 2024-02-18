using Microsoft.Win32;
using StreamGlass.Core.Controls;
using System.Windows;

namespace StreamGlass.Core.Audio
{
    public partial class SoundEditor : Dialog
    {
        private Sound? m_CreatedSound = null;

        public SoundEditor(Controls.Window parent, Sound? audio) : base(parent)
        {
            InitializeComponent();
            AudioFileTextBox.Text = audio?.File ?? string.Empty;
            string[] audioOutputs = SoundManager.GetOutputsNames();
            AudioOutputComboBox.Items.Clear();
            foreach (string output in audioOutputs)
            {
                int idx = AudioOutputComboBox.Items.Add(output);
                if (audio != null && output == audio.Output)
                    AudioOutputComboBox.SelectedIndex = idx;
            }
        }

        public Sound? Sound => m_CreatedSound;

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            object selected = AudioOutputComboBox.SelectedItem;
            m_CreatedSound = new(AudioFileTextBox.Text, (string?)selected ?? string.Empty);
            OnOkClick();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            m_CreatedSound = null;
            OnCancelClick();
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            SoundManager.PlaySound(new(AudioFileTextBox.Text, (string?)AudioOutputComboBox.SelectedItem ?? string.Empty));
        }

        private void AudioFileDialog_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                InitialDirectory = "C:\\",
                Filter = "WAV files (*.wav)|*.wav|MP3 files (*.mp3)|*.mp3|All files (*.*)|*.*",
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() == true)
                AudioFileTextBox.Text = openFileDialog.FileName;
        }
    }
}
