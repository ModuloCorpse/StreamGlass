﻿using CorpseLib.Translation;
using StreamGlass.Core;
using StreamGlass.Core.Controls;
using StreamGlass.Core.Settings;
using System;
using System.Globalization;

namespace StreamGlass.StreamChat
{
    public partial class GeneralSettingsItem : TabItemContent
    {
        private class LanguageInfo(CultureInfo cultureInfo)
        {
            private readonly CultureInfo m_CultureInfo = cultureInfo;
            public CultureInfo CultureInfo => m_CultureInfo;
            public override string ToString() => m_CultureInfo.NativeName;
        }

        private readonly Core.Settings.Dialog m_SettingsDialog;
        private readonly BrushPaletteManager m_BrushPalette;
        private readonly CultureInfo m_OriginalLanguage;
        private readonly string m_OriginalBrushPaletteID;

        public GeneralSettingsItem(Core.Settings.Dialog settingsDialog, BrushPaletteManager brushPalette) : base("${ExeDir}/Assets/tinker.png")
        {
            m_SettingsDialog = settingsDialog;
            m_BrushPalette = brushPalette;
            InitializeComponent();
            ColorModeComboBoxLabel.SetTranslationKey(StreamGlassTranslationKeys.SETTINGS_GENERAL_COLOR);
            LanguageComboBoxLabel.SetTranslationKey(StreamGlassTranslationKeys.SETTINGS_GENERAL_LANGUAGE);
            m_OriginalBrushPaletteID = brushPalette.CurrentObjectID;
            m_OriginalLanguage = Translator.CurrentLanguage;
        }

        protected override void OnInit()
        {
            Helper.FillComboBox(m_BrushPalette, ref ColorModeComboBox, false);

            LanguageComboBox.Items.Clear();
            foreach (CultureInfo obj in Translator.AvailablesLanguages)
            {
                LanguageInfo languageInfo = new(obj);
                LanguageComboBox.Items.Add(languageInfo);
                if (obj.Name == m_OriginalLanguage.Name)
                    LanguageComboBox.SelectedItem = languageInfo;
            }
        }

        private void ColorModeComboBox_SelectionChanged(object sender, EventArgs e)
        {
            m_SettingsDialog!.SetCurrentPalette(((BrushPalette.Info)ColorModeComboBox.SelectedItem).ID);
        }

        private void LanguageComboBox_SelectionChanged(object sender, EventArgs e)
        {
            m_SettingsDialog!.SetCurrentTranslation(((LanguageInfo)LanguageComboBox.SelectedItem).CultureInfo);
        }

        protected override void OnCancel()
        {
            m_SettingsDialog!.SetCurrentPalette(m_OriginalBrushPaletteID);
            m_SettingsDialog!.SetCurrentTranslation(m_OriginalLanguage);
        }
    }
}
