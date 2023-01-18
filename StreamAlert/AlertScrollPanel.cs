using StreamFeedstock;
using StreamFeedstock.Controls;
using StreamGlass.Connections;
using System;

namespace StreamGlass.StreamAlert
{
    public class AlertScrollPanel : ScrollPanel<AlertControl>
    {
        private BrushPaletteManager m_ChatPalette = new();
        private TranslationManager m_Translations = new();
        private ConnectionManager? m_ConnectionManager = null;
        private double m_MessageContentFontSize = 20;

        public AlertScrollPanel() : base()
        {
            CanalManager.Register<Alert>(StreamGlassCanals.ALERT, (int _, object? message) => OnAlert((Alert?)message));
        }

        internal void SetBrushPalette(BrushPaletteManager colorPalette) => m_ChatPalette = colorPalette;

        internal void SetTranslations(TranslationManager translations) => m_Translations = translations;

        public void SetConnectionManager(ConnectionManager connectionManager) => m_ConnectionManager = connectionManager;

        internal double MessageContentFontSize => m_MessageContentFontSize;

        internal void SetContentFontSize(double fontSize)
        {
            m_MessageContentFontSize = fontSize;
            foreach (AlertControl message in Controls)
                message.SetMessageFontSize(m_MessageContentFontSize);
            UpdateControlsPosition();
        }

        private void OnAlert(Alert? alert)
        {
            if (alert == null)
                return;

            Dispatcher.Invoke((Delegate)(() =>
            {
                AlertControl alertMessage = new(m_ConnectionManager!, m_ChatPalette, m_Translations, alert, m_MessageContentFontSize);
                alertMessage.AlertMessage.Loaded += (sender, e) =>
                {
                    alertMessage.UpdateEmotes();
                    UpdateControlsPosition();
                };
                AddControl(alertMessage);
            }));
        }
    }
}
