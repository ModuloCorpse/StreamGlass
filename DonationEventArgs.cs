using StreamGlass.StreamChat;

namespace StreamGlass
{
    public class DonationEventArgs
    {
        private readonly DisplayableMessage m_Message;
        private readonly string m_Name;
        private readonly string m_Currency;
        private readonly float m_Amount;

        public string Name => m_Name;
        public float Amount => m_Amount;
        public string Currency => m_Currency;
        public DisplayableMessage Message => m_Message;

        public DonationEventArgs(string name, float amount, string currency, DisplayableMessage message)
        {
            m_Name = name;
            m_Amount = amount;
            m_Currency = currency;
            m_Message = message;
        }
    }
}
