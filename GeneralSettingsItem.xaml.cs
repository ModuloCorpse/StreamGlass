using StreamGlass.Settings;
using StreamGlass.Controls;
using System;
using System.Globalization;
using CorpseLib.Translation;

namespace StreamGlass.StreamChat
{
    public partial class GeneralSettingsItem : TabItemContent
    {
        private class LanguageInfo
        {
            private readonly CultureInfo m_CultureInfo;
            public CultureInfo CultureInfo => m_CultureInfo;
            public LanguageInfo(CultureInfo cultureInfo) => m_CultureInfo = cultureInfo;
            public override string ToString() => m_CultureInfo.NativeName;
        }

        private readonly BrushPaletteManager m_BrushPalette;
        private readonly CultureInfo m_OriginalLanguage;
        private readonly string m_OriginalBrushPaletteID;

        public GeneralSettingsItem(Data settings, BrushPaletteManager brushPalette) : base("/Assets/tinker.png", "stream-chat", settings)
        {
            m_BrushPalette = brushPalette;
            InitializeComponent();
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
                if (obj == m_OriginalLanguage)
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
