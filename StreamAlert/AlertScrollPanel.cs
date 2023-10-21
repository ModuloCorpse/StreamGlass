﻿using CorpseLib.Placeholder;
using CorpseLib.StructuredText;
using StreamGlass.Controls;
using StreamGlass.Connections;
using StreamGlass.Events;
using System;
using StreamGlass.Stat;

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

        private readonly AlertInfo[] m_GiftAlertInfo = new AlertInfo[Enum.GetNames(typeof(AlertType)).Length];
        private readonly AlertInfo[] m_AlertInfo = new AlertInfo[Enum.GetNames(typeof(AlertType)).Length];
        private BrushPaletteManager m_ChatPalette = new();
        private ConnectionManager? m_ConnectionManager = null;
        private StatisticManager? m_Statistics = null;
        private double m_MessageContentFontSize = 20;

        public AlertScrollPanel() : base()
        {
            StreamGlassCanals.FOLLOW.Register(OnNewFollow);
            StreamGlassCanals.GIFT_FOLLOW.Register(OnNewGiftFollow);
            StreamGlassCanals.DONATION.Register(OnDonation);
            StreamGlassCanals.RAID.Register(OnRaid);
            StreamGlassCanals.REWARD.Register(OnReward);
        }

        internal void SetBrushPalette(BrushPaletteManager colorPalette) => m_ChatPalette = colorPalette;

        public void Init(ConnectionManager connectionManager, StatisticManager statistics)
        {
            m_ConnectionManager = connectionManager;
            m_Statistics = statistics;
        }

        internal double MessageContentFontSize => m_MessageContentFontSize;

        internal void SetContentFontSize(double fontSize)
        {
            m_MessageContentFontSize = fontSize;
            foreach (AlertControl message in Controls)
                message.SetMessageFontSize(m_MessageContentFontSize);
            UpdateControlsPosition();
        }

        internal void SetGiftAlertInfo(AlertType alertType, string imgPath, string prefix, bool isEnabled) => m_GiftAlertInfo[(int)alertType] = new(imgPath, prefix, isEnabled);
        internal void SetGiftAlertInfo(AlertType alertType, AlertInfo alertInfo) => m_GiftAlertInfo[(int)alertType] = new(alertInfo.ImgPath, alertInfo.Prefix, alertInfo.IsEnabled);
        internal AlertInfo GetGiftAlertInfo(AlertType alertType) => m_GiftAlertInfo[(int)alertType];

        internal void SetAlertInfo(AlertType alertType, string imgPath, string prefix, bool isEnabled) => m_AlertInfo[(int)alertType] = new(imgPath, prefix, isEnabled);
        internal void SetAlertInfo(AlertType alertType, AlertInfo alertInfo) => m_AlertInfo[(int)alertType] = new(alertInfo.ImgPath, alertInfo.Prefix, alertInfo.IsEnabled);
        internal AlertInfo GetAlertInfo(AlertType alertType) => m_AlertInfo[(int)alertType];


        private void NewAlert(AlertInfo alertInfo, object e, Text? message = null)
        {
            Dispatcher.Invoke((Delegate)(() =>
            {
                if (alertInfo.IsEnabled)
                {
                    Context context = new();
                    context.AddVariable("e", e);
                    string alertPrefix = (m_Statistics != null) ? Converter.Convert(alertInfo.Prefix, context, m_Statistics) : Converter.Convert(alertInfo.Prefix, context);
                    Text alertMessage = new(alertPrefix);
                    if (message != null)
                        alertMessage.Append(message);
                    Alert alert = new(alertInfo.ImgPath, alertMessage);
                    AlertControl alertControl = new(m_ConnectionManager!, m_ChatPalette, alert, m_MessageContentFontSize);
                    alertControl.AlertMessage.Loaded += (sender, e) => { UpdateControlsPosition(); };
                    AddControl(alertControl);
                }
            }));
        }

        private void OnNewFollow(FollowEventArgs? obj)
        {
            FollowEventArgs e = obj!;
            switch (e.Tier)
            {
                case 0: NewAlert(m_AlertInfo[(int)AlertType.FOLLOW], e, e.Message); break;
                case 1: NewAlert(m_AlertInfo[(int)AlertType.TIER1], e, e.Message); break;
                case 2: NewAlert(m_AlertInfo[(int)AlertType.TIER2], e, e.Message); break;
                case 3: NewAlert(m_AlertInfo[(int)AlertType.TIER3], e, e.Message); break;
                case 4: NewAlert(m_AlertInfo[(int)AlertType.TIER4], e, e.Message); break;
            }
        }

        private void OnNewGiftFollow(GiftFollowEventArgs? obj)
        {
            GiftFollowEventArgs e = obj!;
            switch (e.Tier)
            {
                case 0: NewAlert(m_GiftAlertInfo[(int)AlertType.FOLLOW], e, e.Message); break;
                case 1: NewAlert(m_GiftAlertInfo[(int)AlertType.TIER1], e, e.Message); break;
                case 2: NewAlert(m_GiftAlertInfo[(int)AlertType.TIER2], e, e.Message); break;
                case 3: NewAlert(m_GiftAlertInfo[(int)AlertType.TIER3], e, e.Message); break;
                case 4: NewAlert(m_GiftAlertInfo[(int)AlertType.TIER4], e, e.Message); break;
            }
        }

        private void OnDonation(DonationEventArgs? obj) => NewAlert(m_AlertInfo[(int)AlertType.DONATION], obj!, (obj!).Message);

        private void OnRaid(RaidEventArgs? obj) => NewAlert(m_AlertInfo[(int)((obj!).IsIncomming ? AlertType.INCOMMING_RAID : AlertType.OUTGOING_RAID)], obj!);

        private void OnReward(RewardEventArgs? obj) => NewAlert(m_AlertInfo[(int)AlertType.REWARD], obj!);
    }
}
