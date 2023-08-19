using CorpseLib.Ini;
using CorpseLib.Translation;
using StreamGlass.Controls;
using StreamGlass.Settings;
using System;
using System.Globalization;

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

        public GeneralSettingsItem(IniSection settings, BrushPaletteManager brushPalette) : base("/Assets/tinker.png", settings)
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
