using StreamGlass.Core;
using StreamGlass.Core.Controls;

namespace StreamGlass.Twitch.Alerts
{
    public class AlertScrollPanel : ScrollPanel<AlertControl>
    {
        private double m_MessageContentFontSize = 20;

        public AlertScrollPanel() : base() { }

        public void Init()
        {
            StreamGlassCanals.Register<VisualAlert>(TwitchPlugin.Canals.ALERT, OnVisualAlert);
        }

        internal double MessageContentFontSize => m_MessageContentFontSize;

        public void SetContentFontSize(double fontSize)
        {
            m_MessageContentFontSize = fontSize;
            foreach (AlertControl message in Controls)
                message.SetMessageFontSize(m_MessageContentFontSize);
            UpdateControlsPosition();
        }

        private void OnVisualAlert(VisualAlert? alert)
        {
            Dispatcher.Invoke(() =>
            {
                AlertControl alertControl = new(alert!, m_MessageContentFontSize);
                alertControl.AlertMessage.Loaded += (sender, e) => { UpdateControlsPosition(); };
                AddControl(alertControl);
            });
        }
    }
}
