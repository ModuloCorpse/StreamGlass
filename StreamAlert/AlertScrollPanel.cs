using StreamFeedstock;
using StreamFeedstock.Controls;
using StreamFeedstock.Placeholder;
using StreamGlass.Connections;
using StreamGlass.StreamChat;
using System;
using System.Windows.Controls;

namespace StreamGlass.StreamAlert
{
    public class AlertScrollPanel : ScrollPanel<AlertControl>
    {
        internal enum AlertType
        {
            INCOMMING_RAID,
            OUTGOING_RAID,
            DONATION,
            REWARD,
            FOLLOW,
            TIER1,
            TIER2,
            TIER3,
            TIER4
        }

        internal class AlertInfo
        {
            private readonly string m_ImgPath;
            private readonly string m_Prefix;
            private readonly bool m_IsEnabled;

            public string ImgPath => m_ImgPath;
            public string Prefix => m_Prefix;
            public bool IsEnabled => m_IsEnabled;

            public AlertInfo(string imgPath, string prefix, bool isEnabled)
            {
                m_ImgPath = imgPath;
                m_Prefix = prefix;
                m_IsEnabled = isEnabled;
            }
        }

        private readonly AlertInfo[] m_AlertInfo = new AlertInfo[Enum.GetNames(typeof(AlertType)).Length];
        private BrushPaletteManager m_ChatPalette = new();
        private TranslationManager m_Translations = new();
        private ConnectionManager? m_ConnectionManager = null;
        private double m_MessageContentFontSize = 20;

        public AlertScrollPanel() : base()
        {
            CanalManager.Register<FollowEventArgs>(StreamGlassCanals.FOLLOW, OnNewFollow);
            CanalManager.Register<DonationEventArgs>(StreamGlassCanals.DONATION, OnDonation);
            CanalManager.Register<RaidEventArgs>(StreamGlassCanals.RAID, OnRaid);
            CanalManager.Register<RewardEventArgs>(StreamGlassCanals.REWARD, OnReward);
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

        internal void SetAlertInfo(AlertType alertType, string imgPath, string prefix, bool isEnabled) => m_AlertInfo[(int)alertType] = new(imgPath, prefix, isEnabled);
        internal void SetAlertInfo(AlertType alertType, AlertInfo alertInfo) => m_AlertInfo[(int)alertType] = new(alertInfo.ImgPath, alertInfo.Prefix, alertInfo.IsEnabled);
        internal AlertInfo GetAlertInfo(AlertType alertType) => m_AlertInfo[(int)alertType];


        private void NewAlert(AlertType alertType, object e, DisplayableMessage? message = null)
        {
            Dispatcher.Invoke((Delegate)(() =>
            {
                AlertInfo alertInfo = m_AlertInfo[(int)alertType];
                if (alertInfo.IsEnabled)
                {
                    Context context = new();
                    context.AddVariable("e", e);
                    string alertPrefix = Converter.Convert(alertInfo.Prefix, context);
                    DisplayableMessage alertMessage = (message == null) ? new(alertPrefix) : DisplayableMessage.AppendPrefix(message, alertPrefix);
                    Alert alert = new(alertInfo.ImgPath, alertMessage);
                    AlertControl alertControl = new(m_ConnectionManager!, m_ChatPalette, m_Translations, alert, m_MessageContentFontSize);
                    alertControl.AlertMessage.Loaded += (sender, e) =>
                    {
                        alertControl.UpdateEmotes();
                        UpdateControlsPosition();
                    };
                    AddControl(alertControl);
                }
            }));
        }

        private void OnNewFollow(int _, object? obj)
        {
            FollowEventArgs e = (FollowEventArgs)obj!;
            switch (e.Tier)
            {
                case 0: NewAlert(AlertType.FOLLOW, obj!, e.Message); break;
                case 1: NewAlert(AlertType.TIER1, obj!, e.Message); break;
                case 2: NewAlert(AlertType.TIER2, obj!, e.Message); break;
                case 3: NewAlert(AlertType.TIER3, obj!, e.Message); break;
                case 4: NewAlert(AlertType.TIER4, obj!, e.Message); break;
            }
        }

        private void OnDonation(int _, object? obj) => NewAlert(AlertType.DONATION, obj!, ((DonationEventArgs)obj!).Message);

        private void OnRaid(int _, object? obj) => NewAlert((((RaidEventArgs)obj!).IsIncomming) ? AlertType.INCOMMING_RAID : AlertType.OUTGOING_RAID, obj!);

        private void OnReward(int _, object? obj) => NewAlert(AlertType.REWARD, obj!);
    }
}
