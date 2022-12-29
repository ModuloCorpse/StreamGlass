using StreamFeedstock;
using StreamFeedstock.Controls;
using StreamGlass.StreamChat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace StreamGlass.StreamAlert
{
    public class AlertScrollPanel : ScrollPanel<AlertControl>
    {
        private BrushPaletteManager m_ChatPalette = new();
        private TranslationManager m_Translations = new();
        private double m_MessageContentFontSize = 20;
        private IStreamChat? m_StreamChat = null;

        public AlertScrollPanel() : base()
        {
            CanalManager.Register<Alert>(StreamGlassCanals.ALERT, (int _, object? message) => OnAlert((Alert?)message));
        }

        internal void SetBrushPalette(BrushPaletteManager colorPalette) => m_ChatPalette = colorPalette;

        internal void SetTranslations(TranslationManager translations) => m_Translations = translations;

        public void SetStreamChat(IStreamChat streamChat) => m_StreamChat = streamChat;

        public string GetEmoteURL(string id) => m_StreamChat!.GetEmoteURL(id, m_ChatPalette);

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
                AlertControl alertMessage = new(m_StreamChat!, m_ChatPalette, m_Translations, alert, m_MessageContentFontSize);
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
