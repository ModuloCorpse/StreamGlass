﻿using Microsoft.Win32;
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

            AudioFileTextBoxLabel.SetTranslationKey(StreamGlassTranslationKeys.SOUND_EDITOR_AUDIO_FILE);
            AudioOutputTextBoxLabel.SetTranslationKey(StreamGlassTranslationKeys.SOUND_EDITOR_AUDIO_OUTPUT);
            AudioCooldownTextBoxLabel.SetTranslationKey(StreamGlassTranslationKeys.SOUND_EDITOR_AUDIO_COOLDOWN);
            CloseButton.SetTranslationKey(StreamGlassTranslationKeys.CLOSE_BUTTON);
            SaveButton.SetTranslationKey(StreamGlassTranslationKeys.SAVE_BUTTON);
            TestButton.SetTranslationKey(StreamGlassTranslationKeys.TEST_BUTTON);

            AudioFileTextBox.Text = audio?.File ?? string.Empty;
            AudioCooldownTextBox.Text = audio?.Cooldown.TotalSeconds.ToString() ?? "0";
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
            TimeSpan cooldown = TimeSpan.Zero;
            if (double.TryParse(AudioCooldownTextBox.Text, out double value))
                cooldown = TimeSpan.FromSeconds(value);
            m_CreatedSound = new(AudioFileTextBox.Text, (string?)selected ?? string.Empty, cooldown);
            OnOkClick();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            m_CreatedSound = null;
            OnCancelClick();
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            SoundManager.PlaySound(new(AudioFileTextBox.Text, (string?)AudioOutputComboBox.SelectedItem ?? string.Empty, TimeSpan.Zero));
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
