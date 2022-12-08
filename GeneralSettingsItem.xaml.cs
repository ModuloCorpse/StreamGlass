using StreamGlass.Settings;
using StreamFeedstock.Controls;
using System;
using TabItem = StreamGlass.Settings.TabItem;
using StreamFeedstock;

namespace StreamGlass.StreamChat
{
    public partial class GeneralSettingsItem : TabItem
    {
        private readonly BrushPaletteManager m_BrushPalette;
        private readonly TranslationManager m_Translation;
        private readonly string m_OriginalBrushPaletteID;
        private readonly string m_OriginalTranslationID;

        public GeneralSettingsItem(Data settings, BrushPaletteManager brushPalette, TranslationManager translation) : base("/Assets/tinker.png", "stream-chat", settings)
        {
            m_BrushPalette = brushPalette;
            m_Translation = translation;
            InitializeComponent();
            m_OriginalBrushPaletteID = brushPalette.CurrentObjectID;
            m_OriginalTranslationID = translation.CurrentObjectID;
        }

        protected override void OnInit()
        {
            m_BrushPalette.FillComboBox(ref ColorModeComboBox, false);
            m_Translation.FillComboBox(ref LanguageComboBox, false);
        }

        private void ColorModeComboBox_SelectionChanged(object sender, EventArgs e)
        {
            m_SettingsDialog!.SetCurrentPalette(((BrushPalette.Info)ColorModeComboBox.SelectedItem).ID);
        }

        private void LanguageComboBox_SelectionChanged(object sender, EventArgs e)
        {
            m_SettingsDialog!.SetCurrentTranslation(((Translation.Info)LanguageComboBox.SelectedItem).ID);
        }

        protected override void OnCancel()
        {
            m_SettingsDialog!.SetCurrentPalette(m_OriginalBrushPaletteID);
            m_SettingsDialog!.SetCurrentTranslation(m_OriginalTranslationID);
        }
    }
}
